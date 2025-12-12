using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace FocusFlow.BlazorApp.Auth;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token = null;

        try
        {
            token = await _localStorage.GetItemAsync<string>("authToken", cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Ignored during pre-rendering
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
