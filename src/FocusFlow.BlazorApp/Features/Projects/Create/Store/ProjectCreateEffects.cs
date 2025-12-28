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
            
            // Add the new project to the existing Projects List
             // This ensures the list is updated without needing to reload            
            dispatcher.Dispatch(new AddProjectToListAction(result.Data!));
        }
        else
        {
            dispatcher.Dispatch(new CreateProjectFailureAction(result.Error!));
        }
    }
}