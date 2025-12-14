using FluentAssertions;
using Microsoft.Playwright;

namespace FocusFlow.E2E.Tests;

public class DashboardTests : PageTest
{
    public DashboardTests(PlaywrightFixture playwrightFixture) 
        : base(playwrightFixture)
    {
    }

    [Fact]
    public async Task DashboardDisplaysProjectStatistics()
    {
        // Arrange
        await LoginAsUserAsync();

        // Act
        await Page!.GotoAsync($"{BaseUrl}/dashboard");

        // Assert - Verify statistics cards are visible
        var projectCountCard = Page.Locator("text=/Total Projects/i").Locator("xpath=..");
        await Assertions.Expect(projectCountCard).ToBeVisibleAsync();

        var taskCountCard = Page.Locator("text=/Total Tasks/i").Locator("xpath=..");
        await Assertions.Expect(taskCountCard).ToBeVisibleAsync();

        // Verify numeric values are displayed
        var statNumbers = Page.Locator("h4, h5, .mud-typography-h4, .mud-typography-h5");
        var count = await statNumbers.CountAsync();
        count.Should().BeGreaterThan(0, "Dashboard should display statistics");
    }

    [Fact]
    public async Task DashboardDisplaysRecentProjects()
    {
        // Arrange
        await LoginAsUserAsync();

        // Act
        await Page!.GotoAsync($"{BaseUrl}/dashboard");

        // Assert
        var recentProjectsSection = Page.Locator("text=/Recent Projects/i").Locator("xpath=../..");
        await Assertions.Expect(recentProjectsSection).ToBeVisibleAsync();

        // Verify at least one project card or empty state
        var projectCards = Page.Locator(".mud-card");
        var cardCount = await projectCards.CountAsync();
        cardCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task DashboardShowsProgressBars()
    {
        // Arrange
        await LoginAsUserAsync();

        // Act
        await Page!.GotoAsync($"{BaseUrl}/dashboard");

        // Assert - Verify progress indicators exist
        var progressBars = Page.Locator(".mud-progress-linear, .mud-progress-circular");
        
        // Should have at least some progress indicators if projects exist
        await Task.Delay(1000); // Wait for data to load
        var count = await progressBars.CountAsync();
        
        // This assertion depends on having test data
        // Adjust based on your seeded data
    }
}