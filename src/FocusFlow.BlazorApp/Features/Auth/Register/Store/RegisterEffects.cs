using Fluxor;
using FocusFlow.BlazorApp.Services;
using Microsoft.AspNetCore.Components;

namespace FocusFlow.BlazorApp.Features.Auth.Register.Store;

public class RegisterEffects
{
	private readonly IApiService _apiService;
	private readonly NavigationManager _navigationManager;
	private readonly ILogger<RegisterEffects> _logger;

	public RegisterEffects(
		IApiService apiService,
		NavigationManager navigationManager,
		ILogger<RegisterEffects> logger)
	{
		_apiService = apiService;
		_navigationManager = navigationManager;
		_logger = logger;
	}

	[EffectMethod]
	public async Task HandleRegisterAction(RegisterAction action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Registration attempt. Email='{Email}', Username='{Username}'",
			action.Request.Email,
			action.Request.Username);

		var result = await _apiService.RegisterAsync(action.Request);

		_logger.LogInformation("RegisterAsync returned. Succeeded={Succeeded}, Error='{Error}'",
			result.Succeeded,
			result.Error);

		if (result.Succeeded)
		{
			dispatcher.Dispatch(new RegisterSuccessAction());
			_logger.LogInformation("RegisterSuccessAction dispatched. Navigating to /login");
			_navigationManager.NavigateTo("/login");
		}
		else
		{
			_logger.LogWarning("Registration failed. Error='{Error}'", result.Error);
			dispatcher.Dispatch(new RegisterFailureAction(result.Error!));
		}
	}
}