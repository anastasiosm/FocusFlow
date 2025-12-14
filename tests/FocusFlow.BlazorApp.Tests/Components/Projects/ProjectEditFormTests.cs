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
