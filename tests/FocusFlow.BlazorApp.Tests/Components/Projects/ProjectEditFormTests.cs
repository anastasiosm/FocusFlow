using Bunit;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.BlazorApp.Components.Projects;
using FocusFlow.BlazorApp.Models;
using FocusFlow.BlazorApp.Models.Validators;
using FocusFlow.BlazorApp.Store.Projects;
using Fluxor;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using FluentAssertions;

namespace FocusFlow.BlazorApp.Tests.Components.Projects;

public class ProjectEditFormTests : TestContextBase
{
    private readonly IDispatcher _mockDispatcher;
    private readonly IState<ProjectsState> _mockProjectsState;
    private readonly ISnackbar _mockSnackbar;

    public ProjectEditFormTests()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockProjectsState = Substitute.For<IState<ProjectsState>>();
        _mockSnackbar = Substitute.For<ISnackbar>();
        
        _mockProjectsState.Value.Returns(new ProjectsState(
            isLoading: false,
            error: null,
            projects: new List<ProjectDto>()
        ));

        Services.AddSingleton(_mockDispatcher);
        Services.AddSingleton(_mockProjectsState);
        Services.AddSingleton(_mockSnackbar);
        Services.AddSingleton<IValidator<ProjectUpdateFormModel>>(new ProjectUpdateFormModelValidator());
    }

    [Fact]
    public void ProjectEditForm_ShouldRenderDialogWithCorrectTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<ProjectEditForm>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Project, CreateTestProject())
        );

        // Assert
        var dialog = cut.FindComponent<MudDialog>();
        dialog.Should().NotBeNull();
        
        var title = cut.Find(".mud-dialog-title");
        title.TextContent.Should().Contain("Edit Project");
    }

    [Fact]
    public void ProjectEditForm_ShouldPopulateFieldsWithProjectData()
    {
        // Arrange
        var testProject = CreateTestProject();

        // Act
        var cut = RenderComponent<ProjectEditForm>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Project, testProject)
        );

        // Assert
        var textFields = cut.FindComponents<MudTextField<string>>();
        var nameField = textFields.FirstOrDefault(tf => tf.Instance.Label == "Project Name");
        var descriptionField = textFields.FirstOrDefault(tf => tf.Instance.Label == "Description");
        
        nameField.Should().NotBeNull();
        nameField!.Instance.Value.Should().Be(testProject.Name);
        
        descriptionField.Should().NotBeNull();
        descriptionField!.Instance.Value.Should().Be(testProject.Description);
    }

    [Fact]
    public void ProjectEditForm_ShouldDispatchUpdateActionOnValidSubmit()
    {
        // Arrange
        var testProject = CreateTestProject();
        var cut = RenderComponent<ProjectEditForm>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Project, testProject)
        );

        // Act
        var nameInput = cut.Find("input[type='text']");
        nameInput.Change("Updated Project Name");

        var form = cut.Find("form");
        form.Submit();

        // Assert
        _mockDispatcher.Received(1).Dispatch(Arg.Is<UpdateProjectAction>(
            action => action.Id == testProject.Id
        ));
    }

    [Fact]
    public void ProjectEditForm_ShouldShowLoadingStateWhenUpdating()
    {
        // Arrange
        _mockProjectsState.Value.Returns(new ProjectsState(
            isLoading: true,
            error: null,
            projects: new List<ProjectDto>()
        ));

        // Act
        var cut = RenderComponent<ProjectEditForm>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Project, CreateTestProject())
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        progressCircular.Should().NotBeNull();
        
        var submitButton = cut.Find("button[type='submit']");
        submitButton.TextContent.Should().Contain("Updating");
    }

    [Fact]
    public void ProjectEditForm_ShouldDisableSubmitButtonWhenLoading()
    {
        // Arrange
        _mockProjectsState.Value.Returns(new ProjectsState(
            isLoading: true,
            error: null,
            projects: new List<ProjectDto>()
        ));

        // Act
        var cut = RenderComponent<ProjectEditForm>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Project, CreateTestProject())
        );

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        submitButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ProjectEditForm_ShouldNotDispatchWhenProjectIsNull()
    {
        // Arrange
        var cut = RenderComponent<ProjectEditForm>(parameters => parameters
            .Add(p => p.Visible, true)
            .Add(p => p.Project, null)
        );

        // Act
        var form = cut.Find("form");
        form.Submit();

        // Assert
        _mockDispatcher.DidNotReceive().Dispatch(Arg.Any<UpdateProjectAction>());
    }

    private static ProjectDto CreateTestProject() => new(
        Id: Guid.NewGuid(),
        Name: "Test Project",
        Description: "Test Description",
        OwnerId: Guid.NewGuid().ToString(),
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        TaskCount: 5
    );
}
