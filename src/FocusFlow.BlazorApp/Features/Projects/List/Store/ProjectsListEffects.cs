using Fluxor;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Features.Projects.List.Store;

public class ProjectsListEffects
{
    private readonly IApiService _apiService;

    public ProjectsListEffects(IApiService apiService)
    {
        _apiService = apiService;
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