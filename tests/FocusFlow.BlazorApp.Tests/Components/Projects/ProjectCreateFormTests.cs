using Bunit;
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

public class ProjectCreateFormTests : TestContextBase
{
    private readonly IDispatcher _mockDispatcher;
    private readonly IState<ProjectsState> _mockProjectsState;
    private readonly ISnackbar _mockSnackbar;

    public ProjectCreateFormTests()
    {
        // Mock Fluxor dependencies
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockProjectsState = Substitute.For<IState<ProjectsState>>();
        _mockSnackbar = Substitute.For<ISnackbar>();
        
        // Default state: not loading, no errors
        _mockProjectsState.Value.Returns(new ProjectsState(
            isLoading: false,
            error: null,
            projects: new List<FocusFlow.Application.Features.Projects.Common.ProjectDto>()
        ));

        // Register services
        Services.AddSingleton(_mockDispatcher);
        Services.AddSingleton(_mockProjectsState);
        Services.AddSingleton(_mockSnackbar);
        Services.AddSingleton<IValidator<ProjectCreateFormModel>>(new ProjectCreateFormModelValidator());
    }

    [Fact]
    public void ProjectCreateForm_ShouldRenderDialogWithCorrectTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var dialog = cut.FindComponent<MudDialog>();
        dialog.Should().NotBeNull();
        
        var title = cut.Find(".mud-dialog-title");
        title.TextContent.Should().Contain("Create New Project");
    }

    [Fact]
    public void ProjectCreateForm_ShouldRenderRequiredFields()
    {
        // Arrange & Act
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var textFields = cut.FindComponents<MudTextField<string>>();
        textFields.Should().HaveCountGreaterOrEqualTo(2);
        
        var nameField = textFields.FirstOrDefault(tf => tf.Instance.Label == "Project Name");
        var descriptionField = textFields.FirstOrDefault(tf => tf.Instance.Label == "Description");
        
        nameField.Should().NotBeNull();
        descriptionField.Should().NotBeNull();
    }

    [Fact]
    public void ProjectCreateForm_ShouldDispatchCreateActionOnValidSubmit()
    {
        // Arrange
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Act - Fill in the form
        var nameInput = cut.Find("input[type='text']"); // First input is Name
        nameInput.Change("Test Project Name");

        var form = cut.Find("form");
        form.Submit();

        // Assert
        _mockDispatcher.Received(1).Dispatch(Arg.Any<CreateProjectAction>());
    }

    [Fact]
    public void ProjectCreateForm_ShouldShowLoadingStateWhenCreating()
    {
        // Arrange
        _mockProjectsState.Value.Returns(new ProjectsState(
            isLoading: true,
            error: null,
            projects: new List<FocusFlow.Application.Features.Projects.Common.ProjectDto>()
        ));

        // Act
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var progressCircular = cut.FindComponent<MudProgressCircular>();
        progressCircular.Should().NotBeNull();
        
        var submitButton = cut.Find("button[type='submit']");
        submitButton.TextContent.Should().Contain("Creating");
    }

    [Fact]
    public void ProjectCreateForm_ShouldDisableSubmitButtonWhenLoading()
    {
        // Arrange
        _mockProjectsState.Value.Returns(new ProjectsState(
            isLoading: true,
            error: null,
            projects: new List<FocusFlow.Application.Features.Projects.Common.ProjectDto>()
        ));

        // Act
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        submitButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ProjectCreateForm_ShouldHaveCancelButton()
    {
        // Arrange & Act
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Assert
        var cancelButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancel"));
        cancelButton.Should().NotBeNull();
    }

    [Fact]
    public void ProjectCreateForm_ShouldValidateRequiredName()
    {
        // Arrange
        var cut = RenderComponent<ProjectCreateForm>(parameters => parameters
            .Add(p => p.Visible, true)
        );

        // Act - Submit without filling name
        var form = cut.Find("form");
        form.Submit();

        // Assert - Should NOT dispatch action
        _mockDispatcher.DidNotReceive().Dispatch(Arg.Any<CreateProjectAction>());
    }
}
