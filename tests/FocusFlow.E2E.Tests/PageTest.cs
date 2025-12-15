using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FocusFlow.E2E.Tests;

public abstract class PageTest : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
	protected readonly PlaywrightFixture PlaywrightFixture;
	protected IBrowserContext? Context { get; private set; }
	protected IPage? Page { get; private set; }

	private bool _testFailed = false;
	private string? _videoPath;

	protected virtual string BaseUrl => "http://localhost:5000";

	protected PageTest(PlaywrightFixture playwrightFixture)
	{
		PlaywrightFixture = playwrightFixture;
	}

	public async Task InitializeAsync()
	{
		if (PlaywrightFixture.Browser == null)
			throw new InvalidOperationException("Browser not initialized");

		Context = await PlaywrightFixture.Browser.NewContextAsync(new()
		{
			RecordVideoDir = "videos/",
			RecordVideoSize = new() { Width = 1280, Height = 720 },
			ViewportSize = new() { Width = 1280, Height = 720 },
			IgnoreHTTPSErrors = true
		});

		Page = await Context.NewPageAsync();
		Page.SetDefaultTimeout(30000);
	}

	public async Task DisposeAsync()
	{
		if (Context != null)
		{			
			_videoPath = Page?.Video?.PathAsync().Result;

			await Context.CloseAsync();
			await Context.DisposeAsync();
						
			if (!_testFailed && !string.IsNullOrEmpty(_videoPath) && File.Exists(_videoPath))
			{
				try
				{
					File.Delete(_videoPath);
					Console.WriteLine($"✅ Test passed - deleted video: {_videoPath}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"⚠️ Could not delete video: {ex.Message}");
				}
			}
			else if (_testFailed && !string.IsNullOrEmpty(_videoPath))
			{
				Console.WriteLine($"❌ Test failed - video saved: {_videoPath}");
			}
		}
	}

	protected void MarkTestAsFailed()
	{
		_testFailed = true;
	}

	protected async Task LoginAsUserAsync(string? email = null, string? password = null)
	{
		try
		{
			// ... existing login code ...
		}
		catch (Exception ex)
		{
			_testFailed = true; 
			await CaptureDebugInfoAsync("login-failure");
			throw;
		}
	}

	protected async Task CaptureDebugInfoAsync(string prefix)
	{
		if (Page == null) return;

		try
		{
			var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
			var testName = prefix.Replace(" ", "-").ToLowerInvariant();

			// Save HTML
			var html = await Page.ContentAsync();
			var htmlPath = $"debug-{testName}-{timestamp}.html";
			await File.WriteAllTextAsync(htmlPath, html);
			Console.WriteLine($"💾 HTML saved: {htmlPath}");

			// Save screenshot
			var screenshotPath = $"debug-{testName}-{timestamp}.png";
			await Page.ScreenshotAsync(new PageScreenshotOptions
			{
				Path = screenshotPath,
				FullPage = true
			});
			Console.WriteLine($"📸 Screenshot saved: {screenshotPath}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠️ Failed to capture debug info: {ex.Message}");
		}
	}

	protected async Task<string?> WaitForSnackbarAsync()
	{
		if (Page == null) throw new InvalidOperationException("Page not initialized");

		var snackbar = Page.Locator(".mud-snackbar-content-message");
		await snackbar.WaitForAsync(new LocatorWaitForOptions
		{
			State = WaitForSelectorState.Visible,
			Timeout = 10000
		});
		return await snackbar.TextContentAsync();
	}
}