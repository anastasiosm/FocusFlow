using Bunit;
using FocusFlow.BlazorApp.Features.Projects.List.Components;
using FocusFlow.BlazorApp.Features.Projects.List.Models;
using MudBlazor.Services;
using MudBlazor;
using FluentAssertions;

namespace FocusFlow.BlazorApp.Tests.Components.Projects;

public class ProjectCardTests : TestContextBase
{
    [Fact]
    public void ProjectCard_ShouldRenderProjectDetailsCorrectly()
    {
        // Arrange
        var testProject = new ProjectDto(
            Id: Guid.NewGuid(),
            Name: "Test Project Name",
            Description: "This is a test project description.",
            OwnerId: Guid.NewGuid().ToString(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            TaskCount: 5
        );

        // Act
        var cut = RenderComponent<ProjectCard>(parameters => parameters
            .Add(p => p.Project, testProject)
        );

        // Assert - Using FluentAssertions
        var nameElement = cut.Find("h6");
        nameElement.TextContent.Should().Be(testProject.Name);
        
        var descriptionElement = cut.Find(".mud-typography-body2");
        descriptionElement.TextContent.Trim().Should().Be(testProject.Description);
        
        // Check chip content
        var chipComponent = cut.FindComponent<MudChip<string>>();
        chipComponent.Markup.Should().Contain($"{testProject.TaskCount} tasks");
    }

    [Fact]
    public void ProjectCard_ShouldRenderActionButtons()
    {
        // Arrange
        var testProject = CreateTestProjectDto();

        // Act
        var cut = RenderComponent<ProjectCard>(parameters => parameters
            .Add(p => p.Project, testProject)
        );

        // Assert
        var buttons = cut.FindAll("button");
        buttons.Should().HaveCount(3);
        
        var viewButton = buttons.FirstOrDefault(b => b.TextContent.Contains("View"));
        var editButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Edit"));
        var deleteButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Delete"));
        
        viewButton.Should().NotBeNull();
        editButton.Should().NotBeNull();
        deleteButton.Should().NotBeNull();
    }

    [Fact]
    public void ProjectCard_ShouldInvokeOnViewCallbackWhenViewButtonClicked()
    {
        // Arrange
        var testProject = CreateTestProjectDto();
        var viewCallbackInvoked = false;
        Guid? capturedProjectId = null;

        var cut = RenderComponent<ProjectCard>(parameters => parameters
            .Add(p => p.Project, testProject)
            .Add(p => p.OnView, (projectId) =>
            {
                viewCallbackInvoked = true;
                capturedProjectId = projectId;
            })
        );

        // Act
        var viewButton = cut.FindAll("button").First(b => b.TextContent.Contains("View"));
        viewButton.Click();

        // Assert
        viewCallbackInvoked.Should().BeTrue();
        capturedProjectId.Should().Be(testProject.Id);
    }

    [Fact]
    public void ProjectCard_ShouldInvokeOnEditCallbackWhenEditButtonClicked()
    {
        // Arrange
        var testProject = CreateTestProjectDto();
        var editCallbackInvoked = false;
        Guid? capturedProjectId = null;

        var cut = RenderComponent<ProjectCard>(parameters => parameters
            .Add(p => p.Project, testProject)
            .Add(p => p.OnEdit, (projectId) =>
            {
                editCallbackInvoked = true;
                capturedProjectId = projectId;
            })
        );

        // Act
        var editButton = cut.FindAll("button").First(b => b.TextContent.Contains("Edit"));
        editButton.Click();

        // Assert
        editCallbackInvoked.Should().BeTrue();
        capturedProjectId.Should().Be(testProject.Id);
    }

    [Fact]
    public void ProjectCard_ShouldInvokeOnDeleteCallbackWhenDeleteButtonClicked()
    {
        // Arrange
        var testProject = CreateTestProjectDto();
        var deleteCallbackInvoked = false;
        Guid? capturedProjectId = null;

        var cut = RenderComponent<ProjectCard>(parameters => parameters
            .Add(p => p.Project, testProject)
            .Add(p => p.OnDelete, (projectId) =>
            {
                deleteCallbackInvoked = true;
                capturedProjectId = projectId;
            })
        );

        // Act
        var deleteButton = cut.FindAll("button").First(b => b.TextContent.Contains("Delete"));
        deleteButton.Click();

        // Assert
        deleteCallbackInvoked.Should().BeTrue();
        capturedProjectId.Should().Be(testProject.Id);
    }

    private static ProjectDto CreateTestProjectDto() => new(
        Id: Guid.NewGuid(),
        Name: "Test Project",
        Description: "Test Description",
        OwnerId: Guid.NewGuid().ToString(),
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        TaskCount: 5
    );
}