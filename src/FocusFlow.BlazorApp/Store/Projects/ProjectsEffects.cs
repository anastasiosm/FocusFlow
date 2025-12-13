using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Application.Features.Projects.Common; // For ProjectDto
using FocusFlow.BlazorApp.Components.Pages; // For form models

namespace FocusFlow.BlazorApp.Store.Projects;

public class ProjectsEffects
{
    private readonly IApiService _apiService;
    private readonly IState<ProjectsState> _projectsState;

    public ProjectsEffects(IApiService apiService, IState<ProjectsState> projectsState)
    {
        _apiService = apiService;
        _projectsState = projectsState;
    }

    [EffectMethod]
    public async Task HandleLoadProjectsAction(LoadProjectsAction action, IDispatcher dispatcher)
    {
        var result = await _apiService.GetProjectsAsync();
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadProjectsSuccessAction(result.Data!));
        }
        else
        {
            dispatcher.Dispatch(new LoadProjectsFailureAction(result.Error!));
        }
    }

    [EffectMethod]
    public async Task HandleCreateProjectAction(CreateProjectAction action, IDispatcher dispatcher)
    {
        var createDto = new CreateProjectDto(action.FormModel.Name!, action.FormModel.Description);
        var result = await _apiService.CreateProjectAsync(createDto);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new CreateProjectSuccessAction(result.Data!));
        }
        else
        {
            dispatcher.Dispatch(new CreateProjectFailureAction(result.Error!));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateProjectAction(UpdateProjectAction action, IDispatcher dispatcher)
    {
        var updateDto = new UpdateProjectDto(action.FormModel.Name!, action.FormModel.Description);
        var result = await _apiService.UpdateProjectAsync(action.Id, updateDto);
        if (result.Succeeded)
        {
            // Since API returns no content, construct the updated DTO manually.
            var originalProject = _projectsState.Value.Projects.FirstOrDefault(p => p.Id == action.Id);
            var updatedDto = new ProjectDto(
                action.Id, 
                action.FormModel.Name!, 
                action.FormModel.Description, 
                originalProject?.OwnerId ?? "unknown", // Dummy OwnerId
                originalProject?.CreatedAt ?? DateTime.UtcNow, // Preserve CreatedAt
                DateTime.UtcNow, // UpdatedAt
                originalProject?.TaskCount ?? 0 // Preserve existing task count
            );
            dispatcher.Dispatch(new UpdateProjectSuccessAction(updatedDto));
        }
        else
        {
            dispatcher.Dispatch(new UpdateProjectFailureAction(result.Error!));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteProjectAction(DeleteProjectAction action, IDispatcher dispatcher)
    {
        var result = await _apiService.DeleteProjectAsync(action.Id);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new DeleteProjectSuccessAction(action.Id));
        }
        else
        {
            dispatcher.Dispatch(new DeleteProjectFailureAction(result.Error!));
        }
    }
}
