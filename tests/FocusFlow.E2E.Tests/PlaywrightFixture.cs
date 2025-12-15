using Microsoft.Playwright;

namespace FocusFlow.E2E.Tests;

public class PlaywrightFixture : IAsyncLifetime
{
	public IPlaywright? Playwright { get; private set; }
	public IBrowser? Browser { get; private set; }

	// Configuration from environment variables
	private static bool IsHeadless => Environment.GetEnvironmentVariable("HEADLESS") != "false";
	private static bool IsDebugMode => Environment.GetEnvironmentVariable("DEBUG") == "true";

	public async Task InitializeAsync()
	{
		Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

		var launchOptions = new BrowserTypeLaunchOptions
		{
			// ✅ Headless by default, visible μόνο αν θες debugging
			Headless = IsHeadless,

			// ✅ SlowMo μόνο σε debug mode
			SlowMo = IsDebugMode ? 100 : 0,

			// ✅ Πρόσθετες χρήσιμες ρυθμίσεις
			Args = new[]
			{				
                "--no-sandbox"               // Για Docker
            },

			// ✅ Timeout για browser launch
			Timeout = 30000
		};

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