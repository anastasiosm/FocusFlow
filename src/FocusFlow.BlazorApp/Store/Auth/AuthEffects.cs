using Fluxor;
using FocusFlow.BlazorApp.Services;
using Blazored.LocalStorage;
using FocusFlow.BlazorApp.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims; // For JwtParser
using Microsoft.Extensions.Logging;

namespace FocusFlow.BlazorApp.Store.Auth;

public class AuthEffects
{
    private readonly IApiService _apiService;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<AuthEffects> _logger;

    public AuthEffects(IApiService apiService, ILocalStorageService localStorage, 
                       NavigationManager navigationManager, AuthenticationStateProvider authStateProvider,
                       IDispatcher dispatcher,
                       ILogger<AuthEffects> logger)
    {
        _apiService = apiService;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _authStateProvider = authStateProvider;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoginAction(LoginAction action, IDispatcher dispatcher)
    {
        var result = await _apiService.LoginAsync(action.Request);
        if (result.Succeeded)
        {
            var token = result.Data!;
            var username = JwtParser.ParseClaimsFromJwt(token).FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            await _localStorage.SetItemAsync("authToken", token);
            ((CustomAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(username ?? action.Request.Email);
            dispatcher.Dispatch(new LoginSuccessAction(token, username ?? action.Request.Email));
            _navigationManager.NavigateTo("/projects");
        }
        else
        {
            dispatcher.Dispatch(new LoginFailureAction(result.Error!));
        }
    }

    [EffectMethod]
    public async Task HandleLogoutAction(LogoutAction action, IDispatcher dispatcher)
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        _navigationManager.NavigateTo("/login");
    }

    [EffectMethod]
    public async Task HandleRegisterAction(RegisterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("AuthEffects: HandleRegisterAction started. Email='{Email}', Username='{Username}', PasswordLength={PasswordLength}",
            action.Request.Email,
            action.Request.Username,
            action.Request.Password?.Length ?? 0);

        var result = await _apiService.RegisterAsync(action.Request);

        _logger.LogInformation("AuthEffects: RegisterAsync returned. Succeeded={Succeeded}, Error='{Error}'",
            result.Succeeded,
            result.Error);

        if (result.Succeeded)
        {
            dispatcher.Dispatch(new RegisterSuccessAction());
            _logger.LogInformation("AuthEffects: RegisterSuccessAction dispatched. Navigating to /login");
            _navigationManager.NavigateTo("/login"); // Redirect to login after successful registration
        }
        else
        {
            _logger.LogWarning("AuthEffects: Registration failed. Dispatching RegisterFailureAction. Error='{Error}'", result.Error);
            dispatcher.Dispatch(new RegisterFailureAction(result.Error!));
        }
    }

    [EffectMethod(typeof(Fluxor.StoreInitializedAction))]
    public async Task HandleInitializeStoreAction(IDispatcher dispatcher)
    {
        // Hydrate state from local storage on app load
        var token = await _localStorage.GetItemAsync<string>("authToken");
        string? username = null;
        if (!string.IsNullOrEmpty(token))
        {
            username = JwtParser.ParseClaimsFromJwt(token).FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }
        
        dispatcher.Dispatch(new HydrateAuthStateAction(token, username));
    }
}
