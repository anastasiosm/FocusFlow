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

		// Assert
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard$"));
		await Assertions.Expect(
			Page.GetByText("test@example.com", new() { Exact = false })
		).ToBeVisibleAsync();
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

		// Assert - Wait for error to appear (NOT Task.Delay!)
		var errorMessage = Page.Locator("[data-testid='login-error'], .mud-alert-message").First;
		await Assertions.Expect(errorMessage).ToBeVisibleAsync(new() { Timeout = 5000 });

		// Verify still on login page
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login"));
	}

	[Fact]
	public async Task UserCanLogout()
	{
		// Arrange
		await LoginAsUserAsync();

		// Act
		var logoutButton = Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })
			.Or(Page.GetByRole(AriaRole.Link, new() { Name = "Logout" }));

		await logoutButton.ClickAsync();

		// Assert - Wait for navigation
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login"), new() { Timeout = 10000 });

		// Verify login form is visible
		await Assertions.Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();
	}
}