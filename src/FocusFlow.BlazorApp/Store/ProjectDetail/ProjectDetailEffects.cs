using Fluxor;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public class ProjectDetailEffects
{
    private readonly IApiService _apiService;

    public ProjectDetailEffects(IApiService apiService)
    {
        _apiService = apiService;
    }

    [EffectMethod]
    public async Task HandleLoadProjectDetailAction(LoadProjectDetailAction action, IDispatcher dispatcher)
    {
        var result = await _apiService.GetProjectByIdAsync(action.ProjectId);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoadProjectDetailSuccessAction(result.Data!));
        }
        else
        {
            dispatcher.Dispatch(new LoadProjectDetailFailureAction(result.Error!));
        }
    }
}
