using FocusFlow.BlazorApp.Features.Auth.Register.Models;

namespace FocusFlow.BlazorApp.Features.Auth.Register.Store;

// Register Actions
public record RegisterAction(RegisterRequest Request);
public record RegisterSuccessAction();
public record RegisterFailureAction(string Error);