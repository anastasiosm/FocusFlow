using Fluxor;

namespace FocusFlow.BlazorApp.Features.Auth.Register.Store;

[FeatureState]
public record RegisterState
{
    public bool IsLoading { get; init; }
    public string? Error { get; init; }

    public RegisterState()
    {
        IsLoading = false;
        Error = null;
    }

    public RegisterState(bool isLoading, string? error)
    {
        IsLoading = isLoading;
        Error = error;
    }
}