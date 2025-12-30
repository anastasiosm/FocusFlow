using Fluxor;
using FocusFlow.BlazorApp.Auth;
using FocusFlow.BlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims; // For JwtParser

namespace FocusFlow.BlazorApp.Features.Auth.Login.Store;

/// <summary>
/// Provides authentication-related side effects for login, logout, registration, and authentication state hydration
/// within a Fluxor-based Blazor application.
/// </summary>
/// <remarks>This class contains effect methods that respond to authentication actions by interacting with API
/// services, managing authentication tokens, updating authentication state, and performing navigation. It is intended
/// to be registered with Fluxor's effect system and relies on dependency injection for required services. All effect
/// methods are asynchronous and are triggered by corresponding authentication actions dispatched within the
/// application.</remarks>
public class AuthEffects
{
	private readonly IApiService _apiService;
	private readonly ITokenProvider _tokenProvider;
	private readonly NavigationManager _navigationManager;
	private readonly CustomAuthenticationStateProvider _authStateProvider;
	private readonly ILogger<AuthEffects> _logger;

	public AuthEffects(
		IApiService apiService,
		ITokenProvider tokenProvider,
		NavigationManager navigationManager,
		AuthenticationStateProvider authStateProvider,
		ILogger<AuthEffects> logger)
	{
		_apiService = apiService;
		_tokenProvider = tokenProvider;
		_navigationManager = navigationManager;
		_authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
		_logger = logger;
	}

	[EffectMethod]
	public async Task HandleLoginAction(LoginAction action, IDispatcher dispatcher)
	{
		_logger.LogInformation("Login attempt for email: {Email}", action.Request.Email);

		var result = await _apiService.LoginAsync(action.Request);

		if (result.Succeeded)
		{
			var token = result.Data!;

			try
			{
				var claims = JwtParser.ParseClaimsFromJwt(token);
				var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
							   ?? action.Request.Email;

				// Store token in ITokenProvider (in-memory + ProtectedLocalStorage)
				await _tokenProvider.SetTokenAsync(token);

				// Update authentication state
				await _authStateProvider.MarkUserAsAuthenticatedAsync(token);

				// 👇 Allow Blazor to process auth state + storage
				await Task.Yield();

				// Dispatch success action
				dispatcher.Dispatch(new LoginSuccessAction(token, username));

				_logger.LogInformation("Login successful for user: {Username}", username);

				// Navigate to dashboard
				_navigationManager.NavigateTo("/dashboard");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing login token");
				dispatcher.Dispatch(new LoginFailureAction("An error occurred during login."));
			}
		}
		else
		{
			_logger.LogWarning("Login failed for email: {Email}. Error: {Error}",
				action.Request.Email, result.Error);
			dispatcher.Dispatch(new LoginFailureAction(result.Error!));
		}
	}

	[EffectMethod]
	public async Task HandleLogoutAction(LogoutAction action, IDispatcher dispatcher)
	{
		_logger.LogInformation("User logging out");

		try
		{
			// Clear token from ITokenProvider (in-memory + ProtectedLocalStorage)
			await _tokenProvider.ClearTokenAsync();

			// Update authentication state
			await _authStateProvider.MarkUserAsLoggedOutAsync();

			_logger.LogInformation("Logout successful");

			// Navigate to login
			_navigationManager.NavigateTo("/login");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during logout");
			// Still navigate to login even if there's an error
			_navigationManager.NavigateTo("/login");
		}
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

	[EffectMethod(typeof(Fluxor.StoreInitializedAction))]
	public async Task HandleInitializeStoreAction(IDispatcher dispatcher)
	{
		_logger.LogInformation("Initializing auth state from storage");

		try
		{
			// Get token from TokenProvider (async)
			var token = await _tokenProvider.GetTokenAsync();
			string? username = null;

			if (!string.IsNullOrEmpty(token))
			{
				try
				{
					var claims = JwtParser.ParseClaimsFromJwt(token);
					username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

					_logger.LogInformation("Auth state hydrated for user: {Username}", username);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error parsing token during initialization");
					// Clear invalid token
					await _tokenProvider.ClearTokenAsync();
					token = null;
				}
			}

			dispatcher.Dispatch(new HydrateAuthStateAction(token, username));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during auth state initialization");
			dispatcher.Dispatch(new HydrateAuthStateAction(null, null));
		}
	}
}
