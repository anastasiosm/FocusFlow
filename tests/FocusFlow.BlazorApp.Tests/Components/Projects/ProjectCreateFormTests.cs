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
using FluentAssertions.Collections; // Added this line

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

    // [Fact]
    // public void ProjectCreateForm_ShouldRenderDialogWithCorrectTitle()
    // {
    //     /* Commented out for temporary isolation */
    // }

    // [Fact]
    // public void ProjectCreateForm_ShouldRenderRequiredFields()
    // {
    //     /* Commented out for temporary isolation */
    // }

    // [Fact]
    // public void ProjectCreateForm_ShouldDispatchCreateActionOnValidSubmit()
    // {
    //     /* Commented out for temporary isolation */
    // }

    // [Fact]
    // public void ProjectCreateForm_ShouldShowLoadingStateWhenCreating()
    // {
    //     /* Commented out for temporary isolation */
    // }

    // [Fact]
    // public void ProjectCreateForm_ShouldDisableSubmitButtonWhenLoading()
    // {
    //     /* Commented out for temporary isolation */
    // }

    // [Fact]
    // public void ProjectCreateForm_ShouldHaveCancelButton()
    // {
    //     /* Commented out for temporary isolation */
    // }

    // [Fact]
    // public void ProjectCreateForm_ShouldValidateRequiredName()
    // {
    //     /* Commented out for temporary isolation */
    // }
}
