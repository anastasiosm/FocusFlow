using Microsoft.Playwright;
using System;
using System.Text.RegularExpressions;

namespace FocusFlow.E2E.Tests;

/// <summary>
/// Provides a base class for browser-based Playwright tests, managing browser context and page lifecycle for each test
/// case.
/// </summary>
/// <remarks>This class implements test setup and teardown logic using Playwright, including video recording and
/// cleanup. It is intended to be inherited by test classes that require browser automation. Each test runs in an
/// isolated browser context to prevent state leakage between tests. The class integrates with xUnit's fixture and async
/// lifetime patterns for resource management.</remarks>
public abstract class PageTest : IClassFixture<PlaywrightTestBase>, IAsyncLifetime
{
	protected readonly PlaywrightTestBase PlaywrightFixture;
	protected IBrowserContext? Context { get; private set; }
	protected IPage? Page { get; private set; }

	private bool _testFailed = false;
	private string? _videoPath;

	protected virtual string BaseUrl => "http://localhost:5050";

	protected PageTest(PlaywrightTestBase playwrightFixture)
	{
		PlaywrightFixture = playwrightFixture;
	}

	public async Task InitializeAsync()
	{
		if (PlaywrightFixture.Browser == null)
			throw new InvalidOperationException("Browser not initialized");

		////////////////////////////
		/// Step 3: Create Browser Context
		/// Τι είναι Context:
		/// Σαν "incognito window"
		/// Isolated cookies, localStorage, cache
		/// Κάθε test = fresh context = no pollution
		////////////////////////////
		Context = await PlaywrightFixture.Browser.NewContextAsync(new()
		{
			RecordVideoDir = "videos/", // Directory to save videos
			RecordVideoSize = new() { Width = 1280, Height = 720 },
			ViewportSize = new() { Width = 1280, Height = 720 },
			IgnoreHTTPSErrors = true
			//ScreenshotOnFailure = ScreenshotMode.On  // ← Auto screenshot!
		});

		////////////////////////////
		/// Step 4: Create New Page
		/// Represents one browser tab
		/// Όλα τα actions γίνονται εδώ (click, fill, etc.)
		////////////////////////////
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="email"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	protected async Task LoginAsUserAsync(string? email = null, string? password = null)
	{
		try
		{
			email ??= "test@example.com";
			password ??= "Password123!";

			await Page!.GotoAsync($"{BaseUrl}/login");
			await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

			await Page.GetByLabel("Email").FillAsync(email);
			await Page.GetByLabel("Password").FillAsync(password);
			await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

			// Wait for successful login (redirect to dashboard)
			await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/dashboard$"), new() { Timeout = 10000 });
			
			Console.WriteLine($"✅ Logged in as {email}");
		}
		catch (Exception ex)
		{
			_testFailed = true; 
			await CaptureDebugInfoAsync("login-failure");
			throw new Exception($"Login failed for {email}: {ex.Message}", ex);
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