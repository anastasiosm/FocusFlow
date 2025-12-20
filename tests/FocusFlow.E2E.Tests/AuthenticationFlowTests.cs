using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace FocusFlow.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Infrastructure", "Testcontainers")]
public class AuthenticationFlowTests : PageTest, IClassFixture<E2ETestEnvironment>
{
	protected readonly E2ETestEnvironment TestEnvironment;

	// Override BaseUrl to use Testcontainers URL when available
	protected override string BaseUrl => TestEnvironment?.BlazorBaseUrl ?? "http://localhost:5050";

	public AuthenticationFlowTests(PlaywrightTestBase playwrightFixture, E2ETestEnvironment testEnvironment)
		: base(playwrightFixture)
	{
		TestEnvironment = testEnvironment;
	}

	[Fact]
	public async Task UserCanLoginSuccessfully()
	{
		// Arrange
		Console.WriteLine($"🧪 Testing login against {BaseUrl}");
		Console.WriteLine($"🔗 API URL: {TestEnvironment.ApiBaseUrl}");
		
		// Test API connectivity first
		using var httpClient = new HttpClient();
		try
		{
			var apiResponse = await httpClient.GetAsync($"{TestEnvironment.ApiBaseUrl}/health");
			Console.WriteLine($"🏥 API Health check: {apiResponse.StatusCode}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ API not reachable: {ex.Message}");
		}
		
		await Page!.GotoAsync($"{BaseUrl}/login");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Act
		await Page.GetByLabel("Email").FillAsync("test@example.com");
		await Page.GetByLabel("Password").FillAsync("Password123!");
		
		// Add some debugging
		Console.WriteLine("🔐 Filled credentials, clicking login...");
		await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
		
		// Wait a bit and check for any error messages
		await Task.Delay(2000);
		
		// Check if there are any error messages
		var errorElements = await Page.Locator("[data-testid='login-error'], .mud-alert-message, .alert-danger").AllAsync();
		if (errorElements.Any())
		{
			foreach (var error in errorElements)
			{
				var errorText = await error.TextContentAsync();
				Console.WriteLine($"❌ Login error found: {errorText}");
			}
		}
		
		// Check current URL
		var currentUrl = Page.Url;
		Console.WriteLine($"📍 Current URL after login attempt: {currentUrl}");

		// Assert
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard$"));
		await Assertions.Expect(
			Page.GetByText("test@example.com", new() { Exact = false })
		).ToBeVisibleAsync();

		Console.WriteLine("✅ Login test passed!");
	}

	[Fact]
	public async Task UserCannotLoginWithInvalidCredentials()
	{
		// Arrange
		await Page!.GotoAsync($"{BaseUrl}/login");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Act
		await Page.GetByLabel("Email").FillAsync("wrong@example.com");
		await Page.GetByLabel("Password").FillAsync("WrongPassword!");
		await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

		// Assert - Wait for error to appear
		var errorMessage = Page.Locator("[data-testid='login-error'], .mud-alert-message").First;
		await Assertions.Expect(errorMessage).ToBeVisibleAsync(new() { Timeout = 5000 });

		// Verify still on login page
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login"));

		Console.WriteLine("✅ Invalid login test passed!");
	}

	[Fact]
	public async Task UserCanLogout()
	{
		// Arrange
		await LoginAsUserAsync();

		// Act
		var logoutButton = Page!.GetByRole(AriaRole.Button, new() { Name = "Logout" })
			.Or(Page.GetByRole(AriaRole.Link, new() { Name = "Logout" }));

		await logoutButton.ClickAsync();

		// Assert - Wait for navigation
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login"), new() { Timeout = 10000 });

		// Verify login form is visible
		await Assertions.Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();

		Console.WriteLine("✅ Logout test passed!");
	}
}