using Docker.DotNet.Models;
using Microsoft.Playwright;

namespace FocusFlow.E2E.Tests;

/// <summary>
/// PlaywrightTestBase (Test Helper):
/// * Launches browser & creates pages
/// * Provides helper methods(login, navigate)
/// * Automatic screenshots/videos on failure
/// * Lifecycle management(setup/cleanup)
public class PlaywrightTestBase : IAsyncLifetime
{
	public IPlaywright? Playwright { get; private set; }
	public IBrowser? Browser { get; private set; }

	// Configuration from environment variables
	private static bool IsHeadless => Environment.GetEnvironmentVariable("HEADLESS") != "false";
	private static bool IsDebugMode => Environment.GetEnvironmentVariable("DEBUG") == "true";

	public async Task InitializeAsync()
	{
		// Force visible browser for debugging
		Environment.SetEnvironmentVariable("HEADLESS", "false");
		Environment.SetEnvironmentVariable("DEBUG", "true");
		
		////////////////////////////////
		// Step 1: Initialize Playwright
		///////////////////////////////
		Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

		var launchOptions = new BrowserTypeLaunchOptions
		{
			// ✅ Headless by default, visible μόνο αν θες debugging
			Headless = IsHeadless,          // false = βλέπεις το browser!

			// ✅ SlowMo μόνο σε debug mode
			SlowMo = IsDebugMode ? 100 : 0,  // Delay (ms) για debugging

			// ✅ Πρόσθετες χρήσιμες ρυθμίσεις
			Args = new[]
			{				
                "--no-sandbox"               // Για Docker
				 //"--disable-dev-shm-usage" // Uncomment this For CI/CD .
            },

			// ✅ Timeout για browser launch
			Timeout = 30000
		};

		////////////////////////////////
		// Step 2: Launch Browser
		///////////////////////////////
		// Launch Chromium browser with specified options
		Browser = await Playwright.Chromium.LaunchAsync(launchOptions);

		Console.WriteLine($"🌐 Browser launched: Headless={launchOptions.Headless}, SlowMo={launchOptions.SlowMo}");
	}

	public async Task DisposeAsync()
	{
		if (Browser != null)
		{
			await Browser.CloseAsync();
			await Browser.DisposeAsync();
		}

		Playwright?.Dispose();

		Console.WriteLine("🧹 Browser and Playwright disposed");
	}
}