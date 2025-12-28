using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Features.Projects.Edit.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Store;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Store;

public class ProjectEditEffects
{
    private readonly IApiService _apiService;
    private readonly IState<ProjectsListState> _projectsListState;

    public ProjectEditEffects(IApiService apiService, IState<ProjectsListState> projectsListState)
    {
        _apiService = apiService;
        _projectsListState = projectsListState;
    }

    [EffectMethod]
    public async Task HandleUpdateProjectAction(UpdateProjectAction action, IDispatcher dispatcher)
    {
        var updateDto = new UpdateProjectDto(action.FormModel.Name!, action.FormModel.Description);
        var result = await _apiService.UpdateProjectAsync(action.Id, updateDto);
        
        if (result.Succeeded)
        {
            // Since API returns no content, construct the updated DTO manually.
            var originalProject = _projectsListState.Value.Projects.FirstOrDefault(p => p.Id == action.Id);
            var updatedDto = new ProjectDto(
                action.Id, 
                action.FormModel.Name!, 
                action.FormModel.Description, 
                originalProject?.OwnerId ?? "unknown", // Dummy OwnerId
                originalProject?.CreatedAt ?? DateTime.UtcNow, // Preserve CreatedAt
                DateTime.UtcNow, // UpdatedAt
                originalProject?.TaskCount ?? 0 // Preserve existing task count
            );
            
            // Dispatch success to Edit state
            dispatcher.Dispatch(new UpdateProjectSuccessAction(updatedDto));
            
            // Also update the Projects List state
            // This ensures the list reflects the changes without needing to reload
            var updatedProjects = _projectsListState.Value.Projects
                .Select(p => p.Id == action.Id ? updatedDto : p)
                .ToList();
            dispatcher.Dispatch(new LoadProjectsSuccessAction(updatedProjects));
        }
        else
        {
            dispatcher.Dispatch(new UpdateProjectFailureAction(result.Error!));
        }
    }
}