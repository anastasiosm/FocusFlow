using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Features.Projects.Create.Models;
using FocusFlow.BlazorApp.Features.Projects.List.Store;

namespace FocusFlow.BlazorApp.Features.Projects.Create.Store;

public class ProjectCreateEffects
{
    private readonly IApiService _apiService;

    public ProjectCreateEffects(IApiService apiService)
    {
        _apiService = apiService;
    }

    [EffectMethod]
    public async Task HandleCreateProjectAction(CreateProjectAction action, IDispatcher dispatcher)
    {
        var createDto = new CreateProjectDto(action.FormModel.Name!, action.FormModel.Description);
        var result = await _apiService.CreateProjectAsync(createDto);
        
        if (result.Succeeded)
        {
            // Dispatch success to Create state
            dispatcher.Dispatch(new CreateProjectSuccessAction(result.Data!));
            
            // Also update the Projects List state by adding the new project
            // This ensures the list is updated without needing to reload
            dispatcher.Dispatch(new LoadProjectsSuccessAction(
                new List<FocusFlow.BlazorApp.Features.Projects.List.Models.ProjectDto> { result.Data! }
            ));
        }
        else
        {
            dispatcher.Dispatch(new CreateProjectFailureAction(result.Error!));
        }
    }
}