using Fluxor;

namespace FocusFlow.BlazorApp.Features.Auth.Login.Store;

public record AuthState
{
    public bool IsLoading { get; init; }
    public bool IsAuthenticated { get; init; }
    public string? Username { get; init; }
    public string? Token { get; init; }
    public string? Error { get; init; }

    public AuthState()
    {
        IsLoading = false;
        IsAuthenticated = false;
        Username = null;
        Token = null;
        Error = null;
    }

    public AuthState(bool isLoading, bool isAuthenticated, string? username, string? token, string? error)
    {
        IsLoading = isLoading;
        IsAuthenticated = isAuthenticated;
        Username = username;
        Token = token;
        Error = error;
    }
}

[FeatureState]
public class AuthFeature : Feature<AuthState>
{
    public override string GetName() => "Auth";
    protected override AuthState GetInitialState() => new AuthState();
}
