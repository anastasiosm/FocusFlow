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
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Assert - Verify statistics cards exist with specific data-testid
		var projectCard = Page.Locator("[data-testid='project-statistics-card']");
		await Assertions.Expect(projectCard).ToBeVisibleAsync();

		var taskCard = Page.Locator("[data-testid='task-statistics-card']");
		await Assertions.Expect(taskCard).ToBeVisibleAsync();

		// Verify actual numeric values
		var projectCount = await Page.Locator("[data-testid='total-projects-count']").TextContentAsync();
		var taskCount = await Page.Locator("[data-testid='total-tasks-count']").TextContentAsync();

		int.TryParse(projectCount, out int projects).Should().BeTrue("Project count should be numeric");
		int.TryParse(taskCount, out int tasks).Should().BeTrue("Task count should be numeric");

		projects.Should().BeGreaterThanOrEqualTo(0, "Project count should be non-negative");
		tasks.Should().BeGreaterThanOrEqualTo(0, "Task count should be non-negative");
	}
}