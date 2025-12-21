using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace FocusFlow.E2E.Tests;

// ============================================================
// TESTCONTAINERS CLEANUP - ALL SCENARIOS
// ============================================================

/*
CLEANUP MATRIX:

┌─────────────────────────────────────────────────────────────┐
│ Scenario                    │ Cleanup? │ Mechanism          │
├─────────────────────────────┼──────────┼────────────────────┤
│ Test passes                 │ ✅ YES   │ DisposeAsync()     │
│ Test fails (assertion)      │ ✅ YES   │ DisposeAsync()     │
│ Test throws exception       │ ✅ YES   │ DisposeAsync()     │
│ Test times out              │ ✅ YES   │ DisposeAsync()     │
│ Process crashes (Ctrl+C)    │ ⚠️ MAYBE │ Ryuk (see below)   │
│ Machine crashes/reboot      │ ❌ NO    │ Manual cleanup     │
└─────────────────────────────────────────────────────────────┘
*/

// ============================================================
// SCENARIO 1: NORMAL CLEANUP (Test passes/fails)
// ============================================================

/// <summary>
/// E2ETestEnvironment (Orchestrator)
/// Manages 3 containers: PostgreSQL + API + Client
/// Ensures correct startup order(DB → API → Client)
/// Provides URLs to tests
/// One-time setup per test class
/// </summary>
/// <remarks>This class manages the lifecycle of Docker containers required for end-to-end tests, ensuring that
/// each test runs against a fresh and isolated environment. Containers are started before tests run and are cleaned up
/// automatically after test completion, regardless of test outcome. Implements the IAsyncLifetime interface to
/// integrate with test frameworks that support asynchronous setup and teardown. Manual cleanup may be required if the
/// test process or machine crashes unexpectedly.</remarks>
public class E2ETestEnvironment : IAsyncLifetime
{
	private INetwork _network = null!;
	private PostgreSqlContainer _postgresContainer = null!;
	private IContainer _apiContainer = null!;
	private IContainer _clientContainer = null!;
	private string _tempKeysDir = string.Empty;

	// Public properties to expose URLs to tests
	public string PostgresConnectionString => _postgresContainer?.GetConnectionString() ?? throw new InvalidOperationException("PostgreSQL container not started");
	public string ApiBaseUrl => $"http://localhost:{_apiContainer?.GetMappedPublicPort(8080) ?? throw new InvalidOperationException("API container not started")}";
	public string BlazorBaseUrl => $"http://localhost:{_clientContainer?.GetMappedPublicPort(8080) ?? throw new InvalidOperationException("Blazor container not started")}";

	public async Task InitializeAsync()
	{
		// Disable Ryuk to prevent NullReferenceException in some environments
		Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

		Console.WriteLine("🚀 Starting Testcontainers environment...");

		// Create a custom network for container communication
		_network = new NetworkBuilder()
			.WithName($"focusflow-e2e-{Guid.NewGuid():N}")
			.Build();
		await _network.CreateAsync();

		// Start containers

		// PostgreSQL container
		// * Fresh database per test suite
		// * No data pollution
		// * Production - like(not in -memory)
		_postgresContainer = new PostgreSqlBuilder()
			.WithImage("postgres:17.2")                // ← Ποιο Docker image
			.WithDatabase("focusflow_e2e")             // ← Όνομα DB
			.WithUsername("test")                      // ← Credentials
			.WithPassword("test")
			.WithPortBinding(5432, true)               // ← Random host port!
			.WithNetwork(_network)                     // ← Add to network
			.WithNetworkAliases("postgres")            // ← Network alias for internal communication
			.WithWaitStrategy(
				Wait.ForUnixContainer()
					.UntilPortIsAvailable(5432)        // ← Περίμενε να ξεκινήσει
			)
			.WithCleanUp(true) // ← IMPORTANT: Enable cleanup
			.Build();
		await _postgresContainer.StartAsync();
		Console.WriteLine($"✅ PostgreSQL started: {PostgresConnectionString}");

		// Create temp directory for shared DataProtection keys
		var tempKeysDir = Path.Combine(Path.GetTempPath(), $"focusflow-keys-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempKeysDir);

		// API container
		// * Real ASP.NET app (not WebApplicationFactory)
		// * Tests Docker configuration
		// * Tests environment variables
		_apiContainer = new ContainerBuilder()
			.WithImage("focusflow-api:test")           // ← Το Docker image μας
			.WithPortBinding(8080, true)               // ← Map container port 8080 to random host port
			.WithNetwork(_network)                     // ← Add to network
			.WithNetworkAliases("api")                 // ← Network alias
			.WithBindMount(tempKeysDir, "/tmp/dataprotection-keys")  // ← Shared keys directory
			.WithEnvironment(new Dictionary<string, string>
			{
				// Use network alias for DB connection
				["ConnectionStrings__DefaultConnection"] = "Host=postgres;Port=5432;Database=focusflow_e2e;Username=test;Password=test",
				["JwtSettings__Secret"] = "SuperSecretKeyForTestingPurposesOnly123456789",
				["JwtSettings__Issuer"] = "FocusFlow.Test",
				["JwtSettings__Audience"] = "FocusFlow.Test",
				["JwtSettings__ExpiryMinutes"] = "60",
				["ASPNETCORE_ENVIRONMENT"] = "Development",
				// Test user for E2E tests
				["TestUser__Email"] = "test@example.com",
				["TestUser__Password"] = "Password123!"
			})
			.WithWaitStrategy(
				Wait.ForUnixContainer()
					.UntilPortIsAvailable(8080)        // ← Περίμενε το σωστό port
			)
			.WithCleanUp(true) // ← IMPORTANT: Enable cleanup
			.Build();
		await _apiContainer.StartAsync();
		Console.WriteLine($"✅ API started: {ApiBaseUrl}");

		// Get the API URL for the client to connect to (using internal network alias)
		var ApiUrl = "http://api:8080";

		// Blazor Client container
		// * Real Blazor Server app
		// * Tests static file serving
		// * Tests client-side routing
		_clientContainer = new ContainerBuilder()
			.WithImage("focusflow-client:test")        // ← Blazor Server image
			.WithPortBinding(8080, true)               // ← Map container port 8080 to random host port
			.WithNetwork(_network)                     // ← Add to network
			.WithBindMount(tempKeysDir, "/tmp/dataprotection-keys")  // ← Mount same shared directory
			.WithEnvironment(new Dictionary<string, string>
			{
				["ApiBaseUrl"] = ApiUrl,
				["ASPNETCORE_ENVIRONMENT"] = "Development"
			})
			.WithCleanUp(true) // ← IMPORTANT: Enable cleanup
			.Build();
		await _clientContainer.StartAsync();
		
		// Give Blazor container time to fully start
		await Task.Delay(5000);
		
		Console.WriteLine($"✅ Blazor started: {BlazorBaseUrl}");
		Console.WriteLine("🎉 E2E Test environment ready!");
	}

	public async Task DisposeAsync()
	{
		Console.WriteLine("🧹 Cleaning up Testcontainers environment...");

		// Stop containers in reverse order
		var cleanupTasks = new List<Task>();

		if (_clientContainer != null)
		{
			cleanupTasks.Add(CleanupContainerAsync(_clientContainer, "Blazor"));
		}

		if (_apiContainer != null)
		{
			cleanupTasks.Add(CleanupContainerAsync(_apiContainer, "API"));
		}

		if (_postgresContainer != null)
		{
			cleanupTasks.Add(CleanupContainerAsync(_postgresContainer, "PostgreSQL"));
		}

		await Task.WhenAll(cleanupTasks);

		// Clean up network
		if (_network != null)
		{
			try
			{
				await _network.DeleteAsync();
				Console.WriteLine("✅ Network cleaned up");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠️ Error cleaning up network: {ex.Message}");
			}
		}

		Console.WriteLine("✅ Testcontainers cleanup completed");
	}

	private static async Task CleanupContainerAsync(IContainer container, string name)
	{
		try
		{
			await container.StopAsync();
			await container.DisposeAsync();
			Console.WriteLine($"✅ {name} container cleaned up");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠️ Error cleaning up {name} container: {ex.Message}");
		}
	}
}

// ============================================================
// WHY CLEANUP WORKS EVEN ON FAILURE
// ============================================================

/*
xUnit Lifecycle:

1. Test Class Created
2. Constructor runs
3. IAsyncLifetime.InitializeAsync() ← Start containers
4. Test Method runs
   └─ IF FAILS → xUnit catches exception
5. IAsyncLifetime.DisposeAsync() ← ALWAYS runs (finally block)
6. Test Class Disposed

Key Point: DisposeAsync() is in a "finally" block internally!
*/

//// Example: Even with assertion failure
//[Fact]
//public async Task Test_ThatFails()
//{
//	// Containers started
//	await Page.GotoAsync($"{ClientUrl}/login");

//	// This fails!
//	Assert.Equal("Wrong", "Expected"); // ❌ FAILS

//	// DisposeAsync() STILL RUNS!
//	// Containers are cleaned up ✅
//}

// ============================================================
// SCENARIO 2: RYUK - THE CLEANUP DAEMON
// ============================================================

/*
What is Ryuk?

Ryuk is a "Resource Reaper" that Testcontainers starts automatically.

How it works:
1. Testcontainers starts Ryuk container
2. Ryuk monitors your test containers
3. If test process dies unexpectedly, Ryuk kills containers
4. Prevents "zombie containers"

Docker command:
docker run -d --rm \
  -v /var/run/docker.sock:/var/run/docker.sock \
  testcontainers/ryuk:0.7.0
*/

// Enable Ryuk (DEFAULT)
//var container = new PostgreSqlBuilder()
//	.WithCleanUp(true)  // ← Enables Ryuk
//	.Build();

//// Disable Ryuk (NOT RECOMMENDED)
//var container = new PostgreSqlBuilder()
//	.WithCleanUp(false) // ← Containers stay after crash!
//	.Build();

// ============================================================
// SCENARIO 3: PROCESS CRASH (Ctrl+C, kill, etc.)
// ============================================================

/*
What happens:

1. You press Ctrl+C during test
2. .NET process terminates immediately
3. DisposeAsync() DOES NOT RUN ❌
4. Ryuk detects connection loss
5. Ryuk kills all containers ✅

Timeline:
[00:00] Test running
[00:05] User presses Ctrl+C
[00:05] Process dies
[00:05] DisposeAsync() NOT called
[00:06] Ryuk detects disconnection
[00:07] Ryuk runs: docker stop container1 container2 container3
[00:08] Containers cleaned up ✅
*/

// Verify Ryuk is running:
// docker ps | grep ryuk
// Output: testcontainers/ryuk:0.7.0

// ============================================================
// SCENARIO 4: MACHINE CRASH/REBOOT
// ============================================================

/*
What happens:

1. Machine loses power / hard reboot
2. All Docker processes stop
3. Containers are stopped BUT NOT REMOVED
4. On next boot, containers exist but are stopped

Result: "Zombie containers" 🧟

How to verify:
docker ps -a | grep focusflow
# Shows: Exited (137) 2 hours ago

Manual cleanup:
docker container prune -f
# OR
docker rm -f $(docker ps -aq)
*/

