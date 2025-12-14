using FluentAssertions;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FocusFlow.E2E.Tests;

public class ProjectManagementFlowTests : PageTest
{
    public ProjectManagementFlowTests(PlaywrightFixture playwrightFixture) 
        : base(playwrightFixture)
    {
    }

    [Fact]
    public async Task UserCanCreateNewProject()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");

        // Act - Open create dialog
        await Page.ClickAsync("button:has-text('Create Project'), button:has-text('New Project')");
        
        // Wait for MudBlazor dialog
        await Page.WaitForSelectorAsync(".mud-dialog");

        // Fill form
        await Page.FillAsync("input[id*='Name'], input[label='Project Name']", "E2E Test Project");
        await Page.FillAsync("textarea[id*='Description'], textarea[label='Description']", "Created by E2E test");
        
        // Submit
        await Page.ClickAsync("button:has-text('Create'):visible");

        // Assert
        var snackbarMessage = await WaitForSnackbarAsync();
        snackbarMessage.Should().Contain("successfully", "Project should be created successfully");

        // Verify project appears in list
        var projectCard = Page.Locator("text=E2E Test Project");
        await Assertions.Expect(projectCard).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserCanViewProjectDetails()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");

        // Act - Click first project card
        var firstProject = Page.Locator(".mud-card").First;
        await firstProject.ClickAsync();

        // Assert - Should navigate to project details
        await Page.WaitForURLAsync(new Regex(".*/projects/[0-9a-f-]+.*"));
        
        // Verify project details page elements
        var projectTitle = Page.Locator("h4, h5").First;
        await Assertions.Expect(projectTitle).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserCanEditProject()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");

        // Act - Open edit dialog (adjust selector based on your UI)
        await Page.Locator(".mud-card").First.Locator("button[title*='Edit'], button:has-text('Edit')").ClickAsync();
        
        await Page.WaitForSelectorAsync(".mud-dialog");

        // Modify name
        var nameInput = Page.Locator("input[id*='Name'], input[label='Project Name']");
        await nameInput.FillAsync("Updated E2E Project");
        
        // Submit
        await Page.ClickAsync("button:has-text('Save'):visible, button:has-text('Update'):visible");

        // Assert
        var snackbarMessage = await WaitForSnackbarAsync();
        snackbarMessage.Should().Contain("successfully", "Update should succeed");
    }

    [Fact]
    public async Task UserCanDeleteProject()
    {
        // Arrange
        await LoginAsUserAsync();
        await Page!.GotoAsync($"{BaseUrl}/projects");

        var projectCard = Page.Locator(".mud-card").First;
        var projectName = await projectCard.Locator("h6, .mud-card-header").TextContentAsync();

        // Act - Click delete button
        await projectCard.Locator("button[title*='Delete'], button:has-text('Delete')").ClickAsync();
        
        // Confirm deletion in dialog
        await Page.WaitForSelectorAsync(".mud-dialog");
        await Page.ClickAsync("button:has-text('Delete'):visible, button:has-text('Confirm'):visible");

        // Assert
        var snackbarMessage = await WaitForSnackbarAsync();
        snackbarMessage.Should().Contain("deleted", "Delete should succeed");

        // Verify project removed
        var deletedProject = Page.Locator($"text={projectName}");
        await Assertions.Expect(deletedProject).Not.ToBeVisibleAsync();
    }
}