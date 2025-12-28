using Fluxor;
using FocusFlow.BlazorApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace FocusFlow.BlazorApp.Features.Projects.Detail.Store;

public class ProjectDetailEffects
{
	private readonly IApiService _apiService;
	private readonly AuthenticationStateProvider _authStateProvider;

	public ProjectDetailEffects(
		IApiService apiService,
		AuthenticationStateProvider authStateProvider)
	{
		_apiService = apiService;
		_authStateProvider = authStateProvider;
	}

	[EffectMethod]
	public async Task HandleLoadProjectDetailAction(
		LoadProjectDetailAction action,
		IDispatcher dispatcher)
	{
		var authState = await _authStateProvider.GetAuthenticationStateAsync();

		if (!authState.User.Identity?.IsAuthenticated ?? true)
		{
			// ⚠️ Fallback: If called too early, dispatch failure instead of silent return
			dispatcher.Dispatch(new LoadProjectDetailFailureAction("User not authenticated"));
			return;
		}

		var result = await _apiService.GetProjectByIdAsync(action.ProjectId);

		if (result.Succeeded)
			dispatcher.Dispatch(new LoadProjectDetailSuccessAction(result.Data!));
		else
			dispatcher.Dispatch(new LoadProjectDetailFailureAction(result.Error!));
	}
}
