using FocusFlow.BlazorApp.Features.Auth.Login.Models;
using FocusFlow.BlazorApp.Features.Auth.Register.Models;

namespace FocusFlow.BlazorApp.Features.Auth.Login.Store;

// Login Actions
public record LoginAction(LoginRequest Request);
public record LoginSuccessAction(string Token, string Username);
public record LoginFailureAction(string Error);

// Logout Actions
public record LogoutAction();

// Register Actions
public record RegisterAction(RegisterRequest Request);
public record RegisterSuccessAction();
public record RegisterFailureAction(string Error);

// Hydrate State on App Load (from Local Storage)
public record HydrateAuthStateAction(string? Token, string? Username);
