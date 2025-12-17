using FluentAssertions;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FocusFlow.E2E.Tests;

[Trait("Category", "E2E")]
public class ProjectManagementFlowTests : PageTest, IAsyncLifetime
{
	private string _testProjectId = string.Empty;
	private readonly string _uniqueProjectName = $"E2E-Test-{Guid.NewGuid().ToString()[..8]}";

	public ProjectManagementFlowTests(PlaywrightFixture playwrightFixture)
		: base(playwrightFixture)
	{
	}

	// ✅ Setup - Δημιουργεί test data πριν από κάθε test
	public new async Task InitializeAsync()
	{
		await base.InitializeAsync();
		await LoginAsUserAsync();
	}

	// ✅ Cleanup - Καθαρίζει test data μετά από κάθε test
	public new async Task DisposeAsync()
	{
		// Cleanup test project if created
		if (!string.IsNullOrEmpty(_testProjectId))
		{
			try
			{
				await CleanupProjectAsync(_testProjectId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠️ Cleanup failed: {ex.Message}");
			}
		}

		await base.DisposeAsync();
	}

	[Fact]
	public async Task UserCanCreateNewProject()
	{
		// Arrange
		await Page!.GotoAsync($"{BaseUrl}/projects");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Act - Open create dialog with specific selector
		var createButton = Page.GetByRole(AriaRole.Button, new() { Name = "Create Project" })
			.Or(Page.GetByRole(AriaRole.Button, new() { Name = "New Project" }));

		await createButton.ClickAsync();

		// Wait for dialog
		var dialog = Page.Locator("[role='dialog'], .mud-dialog");
		await Assertions.Expect(dialog).ToBeVisibleAsync();

		// Fill form with unique name
		await Page.GetByLabel("Project Name")
			.Or(Page.Locator("input[placeholder*='name' i]"))
			.FillAsync(_uniqueProjectName);

		await Page.GetByLabel("Description")
			.Or(Page.Locator("textarea[placeholder*='description' i]"))
			.FillAsync("Created by automated E2E test");

		// Submit
		var submitButton = dialog.GetByRole(AriaRole.Button, new() { Name = "Create" });
		await submitButton.ClickAsync();

		// Assert - Wait for success message
		var snackbar = await WaitForSnackbarAsync();
		snackbar.Should().Contain("success", because: "project creation should succeed");

		// ✅ Store project ID for cleanup
		await Page.WaitForURLAsync(new Regex(".*/projects/([0-9a-f-]+)"));
		_testProjectId = ExtractProjectIdFromUrl(Page.Url);

		// Verify project appears
		var projectCard = Page.GetByText(_uniqueProjectName);
		await Assertions.Expect(projectCard).ToBeVisibleAsync();
	}

	[Fact]
	public async Task UserCanViewProjectDetails()
	{
		// ✅ Arrange - Create test project first
		var projectId = await CreateTestProjectAsync(_uniqueProjectName);
		_testProjectId = projectId;

		// Act
		await Page!.GotoAsync($"{BaseUrl}/projects");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Click on our specific test project
		var projectCard = Page.GetByText(_uniqueProjectName);
		await projectCard.ClickAsync();

		// Assert - Navigate to details page
		await Assertions.Expect(Page).ToHaveURLAsync(new Regex($".*/projects/{projectId}"));

		// Verify details page loaded
		var pageTitle = Page.GetByRole(AriaRole.Heading, new() { Name = _uniqueProjectName })
			.Or(Page.Locator("h1, h2, h3, h4").Filter(new() { HasText = _uniqueProjectName }));

		await Assertions.Expect(pageTitle).ToBeVisibleAsync();
	}

	[Fact]
	public async Task UserCanEditProject()
	{
		// ✅ Arrange - Create test project
		var projectId = await CreateTestProjectAsync(_uniqueProjectName);
		_testProjectId = projectId;

		await Page!.GotoAsync($"{BaseUrl}/projects/{projectId}");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Act - Open edit dialog
		var editButton = Page.GetByRole(AriaRole.Button, new() { Name = "Edit" })
			.Or(Page.Locator("button[title*='Edit' i]"));

		await editButton.ClickAsync();

		var dialog = Page.Locator("[role='dialog'], .mud-dialog");
		await Assertions.Expect(dialog).ToBeVisibleAsync();

		// Modify name
		var updatedName = $"{_uniqueProjectName}-Updated";
		var nameInput = dialog.GetByLabel("Project Name")
			.Or(dialog.Locator("input[id*='name' i]"));

		await nameInput.ClearAsync();
		await nameInput.FillAsync(updatedName);

		// Submit
		var saveButton = dialog.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Save|Update") });
		await saveButton.ClickAsync();

		// Assert
		var snackbar = await WaitForSnackbarAsync();
		snackbar.Should().Contain("success", because: "update should succeed");

		// Verify updated name appears
		var updatedTitle = Page.GetByText(updatedName);
		await Assertions.Expect(updatedTitle).ToBeVisibleAsync();
	}

	[Fact]
	public async Task UserCanDeleteProject()
	{
		// ✅ Arrange - Create test project
		var projectId = await CreateTestProjectAsync(_uniqueProjectName);
		_testProjectId = projectId; // Will be cleaned up automatically

		await Page!.GotoAsync($"{BaseUrl}/projects");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		// Act - Find and delete our specific project
		var projectCard = Page.GetByText(_uniqueProjectName).Locator("..");
		var deleteButton = projectCard.GetByRole(AriaRole.Button, new() { Name = "Delete" })
			.Or(projectCard.Locator("button[title*='Delete' i]"));

		await deleteButton.ClickAsync();

		// Confirm deletion
		var confirmDialog = Page.Locator("[role='dialog'], .mud-dialog");
		await Assertions.Expect(confirmDialog).ToBeVisibleAsync();

		var confirmButton = confirmDialog.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Delete|Confirm") });
		await confirmButton.ClickAsync();

		// Assert
		var snackbar = await WaitForSnackbarAsync();
		snackbar.Should().MatchRegex("delet.*success", because: "deletion should succeed");

		// Verify project removed from list
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		var deletedProject = Page.GetByText(_uniqueProjectName);
		await Assertions.Expect(deletedProject).Not.ToBeVisibleAsync();

		// ✅ Clear ID since it's already deleted
		_testProjectId = string.Empty;
	}

	// ✅ Helper methods
	private async Task<string> CreateTestProjectAsync(string name)
	{
		await Page!.GotoAsync($"{BaseUrl}/projects");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		var createButton = Page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Create|New") });
		await createButton.ClickAsync();

		var dialog = Page.Locator("[role='dialog']");
		await dialog.GetByLabel("Project Name").FillAsync(name);
		await dialog.GetByLabel("Description").FillAsync("Test project for E2E");

		await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
		await WaitForSnackbarAsync();

		// Extract project ID from URL
		await Page.WaitForURLAsync(new Regex(".*/projects/([0-9a-f-]+)"));
		return ExtractProjectIdFromUrl(Page.Url);
	}

	private async Task CleanupProjectAsync(string projectId)
	{
		try
		{
			await Page!.GotoAsync($"{BaseUrl}/projects/{projectId}");

			var deleteButton = Page.GetByRole(AriaRole.Button, new() { Name = "Delete" });
			if (await deleteButton.CountAsync() > 0)
			{
				await deleteButton.ClickAsync();
				var confirmButton = Page.Locator("[role='dialog']")
					.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Delete|Confirm") });
				await confirmButton.ClickAsync();
				await Task.Delay(1000); // Wait for deletion
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠️ Could not cleanup project {projectId}: {ex.Message}");
		}
	}

	private string ExtractProjectIdFromUrl(string url)
	{
		var match = Regex.Match(url, @"projects/([0-9a-f-]+)");
		return match.Success ? match.Groups[1].Value : string.Empty;
	}
}