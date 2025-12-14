using Microsoft.Playwright;

namespace FocusFlow.E2E.Tests;

public abstract class PageTest : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    protected readonly PlaywrightFixture PlaywrightFixture;
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }
    
    protected virtual string BaseUrl => "http://localhost:5000";
    
    protected PageTest(PlaywrightFixture playwrightFixture)
    {
        PlaywrightFixture = playwrightFixture;
    }

    public async Task InitializeAsync()
    {
        if (PlaywrightFixture.Browser == null)
            throw new InvalidOperationException("Browser not initialized");

        Context = await PlaywrightFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            RecordVideoDir = "videos/",
            RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 },
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true
        });

        Page = await Context.NewPageAsync();
        
        // Increase timeout for Blazor applications
        Page.SetDefaultTimeout(30000);
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.CloseAsync();
            await Context.DisposeAsync();
        }
    }

    protected async Task LoginAsUserAsync(string email = "test@example.com", string password = "Password123!")
    {
        if (Page == null) throw new InvalidOperationException("Page not initialized");

        await Page.GotoAsync($"{BaseUrl}/login", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        // Wait for Blazor to render
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);

        try
        {
            // Try accessible queries first (works with MudBlazor labels)
            var emailLocator = Page.GetByLabel("Email");
            if (await emailLocator.CountAsync() == 0)
            {
                // Fallback: common input selectors including MudBlazor structure
                emailLocator = Page.Locator("input[type='email'], input[type='text'], .mud-input-root input").First;
            }

            await emailLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            await emailLocator.FillAsync(email);

            var passwordLocator = Page.GetByLabel("Password");
            if (await passwordLocator.CountAsync() == 0)
            {
                passwordLocator = Page.Locator("input[type='password'], .mud-input-root input[type='password']").First;
            }

            await passwordLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            await passwordLocator.FillAsync(password);

            // Click login button (try role-based then text)
            var loginButton = Page.GetByRole(AriaRole.Button, new() { Name = "Login" });
            if (await loginButton.CountAsync() == 0)
            {
                await Page.ClickAsync("button:has-text('Login')");
            }
            else
            {
                await loginButton.ClickAsync();
            }

            // Wait for dashboard or some indication of success
            await Page.WaitForURLAsync($"{BaseUrl}/dashboard", new PageWaitForURLOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000 });
        }
        catch (Exception ex)
        {
            // Dump HTML and screenshot for debugging
            try
            {
                var html = await Page.ContentAsync();
                await File.WriteAllTextAsync("debug-login-failure.html", html);
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = "debug-login-failure.png", FullPage = true });
            }
            catch { }

            throw new InvalidOperationException($"Login flow failed: {ex.Message}", ex);
        }
    }

    protected async Task<string?> WaitForSnackbarAsync()
    {
        if (Page == null) throw new InvalidOperationException("Page not initialized");
        
        var snackbar = Page.Locator(".mud-snackbar-content-message");
        await snackbar.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30000
        });
        return await snackbar.TextContentAsync();
    }
}