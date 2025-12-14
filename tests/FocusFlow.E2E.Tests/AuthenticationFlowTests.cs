using FluentAssertions;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FocusFlow.E2E.Tests;

public class AuthenticationFlowTests : PageTest
{
    public AuthenticationFlowTests(PlaywrightFixture playwrightFixture) 
        : base(playwrightFixture)
    {
    }

    // [Fact]
    // public async Task UserCanLoginSuccessfully()
    // {
    //     // Arrange & Act
    //     await Page!.GotoAsync($"{BaseUrl}/login");
    //     await Page.FillAsync("input[type='email']", "test@example.com");
    //     await Page.FillAsync("input[type='password']", "Password123!");
    //     await Page.ClickAsync("button[type='submit']");

    //     // Assert
    //     await Assertions.Expect(Page).ToHaveURLAsync($"{BaseUrl}/dashboard");
        
    //     // Verify user is logged in (check for user menu or indicator)
    //     var userIndicator = Page.Locator("text=/test@example.com/i");
    //     await Assertions.Expect(userIndicator).ToBeVisibleAsync();
    // }

    [Fact]
public async Task UserCanLoginSuccessfully()
{
    // Arrange
    await Page!.GotoAsync($"{BaseUrl}/login");
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Act
    await Page.GetByLabel("Email").FillAsync("test@example.com");
    await Page.GetByLabel("Password").FillAsync("Password123!");

    await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

    // Assert - περιμένουμε redirect
    await Page.WaitForURLAsync($"{BaseUrl}/dashboard");

    // Assert - logged-in indicator
    await Assertions.Expect(
        Page.GetByText("test@example.com", new() { Exact = false })
    ).ToBeVisibleAsync();
}


    [Fact]
    public async Task UserCannotLoginWithInvalidCredentials()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/login");
        await Page.FillAsync("input[type='email']", "wrong@example.com");
        await Page.FillAsync("input[type='password']", "WrongPassword!");
        await Page.ClickAsync("button[type='submit']");

        // Assert - should remain on login page
        await Task.Delay(1000); // Wait for any validation
        var currentUrl = Page.Url;
        currentUrl.Should().Contain("/login");
        
        // Check for error message (adjust selector based on your UI)
        var errorMessage = Page.Locator(".mud-alert-message, .validation-message");
        await Assertions.Expect(errorMessage).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserCanLogout()
    {
        // Arrange
        await LoginAsUserAsync();

        // Act - Click logout button (adjust selector based on your UI)
        await Page!.ClickAsync("button:has-text('Logout'), a:has-text('Logout')");

        // Assert
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login.*"));
    }
}