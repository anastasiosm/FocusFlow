using FluentAssertions;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FocusFlow.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Infrastructure", "Testcontainers")]
public class TaskManagementFlowTests : PageTest, IClassFixture<E2ETestEnvironment>
{
    protected readonly E2ETestEnvironment TestEnvironment;

    // Override BaseUrl to use Testcontainers URL when available
    protected override string BaseUrl => TestEnvironment?.BlazorBaseUrl ?? "http://localhost:5050";

    public TaskManagementFlowTests(PlaywrightTestBase playwrightFixture, E2ETestEnvironment testEnvironment) 
        : base(playwrightFixture)
    {
        TestEnvironment = testEnvironment;
    }

    [Fact]
    public async Task UserCanCreateTask()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");
        
        // Navigate to first project
        await Page.Locator(".mud-card").First.ClickAsync();
        await Page.WaitForURLAsync(new Regex(".*/projects/.*"));

        // Act - Open create task dialog
        await Page.ClickAsync("button:has-text('Add Task'), button:has-text('Create Task')");
        await Page.WaitForSelectorAsync(".mud-dialog");

        // Fill task form
        await Page.FillAsync("input[id*='Title'], input[label='Title']", "E2E Test Task");
        await Page.FillAsync("textarea[id*='Description'], textarea[label='Description']", "Task created by E2E test");
        
        // Select priority if available
        var prioritySelect = Page.Locator("select[id*='Priority'], div.mud-select:has-text('Priority')");
        if (await prioritySelect.CountAsync() > 0)
        {
            await prioritySelect.ClickAsync();
            await Page.ClickAsync("div.mud-list-item:has-text('High')");
        }

        // Submit
        await Page.ClickAsync("button:has-text('Create'):visible");

        // Assert
        var snackbarMessage = await WaitForSnackbarAsync();
        snackbarMessage.Should().Contain("successfully", "Task should be created successfully");

        // Verify task appears in ToDo column
        var todoColumn = Page.Locator("[data-column='ToDo'], .todo-column, div:has-text('To Do')").First;
        var taskCard = todoColumn.Locator("text=E2E Test Task");
        await Assertions.Expect(taskCard).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserCanMoveTaskBetweenColumns()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");
        await Page.Locator(".mud-card").First.ClickAsync();
        await Page.WaitForURLAsync(new Regex(".*/projects/.*"));

        // Find a task in ToDo column
        var todoColumn = Page.Locator("[data-column='ToDo'], .todo-column").First;
        var taskCard = todoColumn.Locator(".mud-card, .task-card").First;

        if (await taskCard.CountAsync() == 0)
        {
            // Create a task first if none exist
            await Page.ClickAsync("button:has-text('Add Task')");
            await Page.WaitForSelectorAsync(".mud-dialog");
            await Page.FillAsync("input[label='Title']", "Drag Test Task");
            await Page.ClickAsync("button:has-text('Create'):visible");
            await WaitForSnackbarAsync();
            taskCard = todoColumn.Locator(".mud-card, .task-card").First;
        }

        // Act - Drag to InProgress column
        var inProgressColumn = Page.Locator("[data-column='InProgress'], .inprogress-column").First;
        await taskCard.DragToAsync(inProgressColumn);

        // Assert - Verify task moved
        await Task.Delay(500); // Wait for drag animation
        var movedTask = inProgressColumn.Locator(".mud-card, .task-card").First;
        await Assertions.Expect(movedTask).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserCanEditTask()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");
        await Page.Locator(".mud-card").First.ClickAsync();

        // Act - Click edit on first task
        var firstTaskCard = Page.Locator(".task-card, .mud-card").First;
        await firstTaskCard.Locator("button[title*='Edit']").ClickAsync();
        
        await Page.WaitForSelectorAsync(".mud-dialog");

        // Modify title
        var titleInput = Page.Locator("input[id*='Title'], input[label='Title']");
        await titleInput.FillAsync("Updated Task Title");
        
        // Submit
        await Page.ClickAsync("button:has-text('Save'):visible");

        // Assert
        var snackbarMessage = await WaitForSnackbarAsync();
        snackbarMessage.Should().Contain("successfully");
    }
}