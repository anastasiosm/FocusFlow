namespace FocusFlow.BlazorApp.Services;

public interface ITokenProvider
{
    // Async methods for ProtectedLocalStorage
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string? token);
    Task ClearTokenAsync();

    // Backward compatibility methods
    string? GetToken();
    void SetToken(string? token);
    void ClearToken();
}
