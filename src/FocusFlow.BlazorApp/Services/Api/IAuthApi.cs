using FocusFlow.BlazorApp.Models;
using Refit;

namespace FocusFlow.BlazorApp.Services.Api;

public interface IAuthApi
{
    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest request);

    [Post("/api/auth/register")]
    Task RegisterAsync([Body] RegisterRequest request);
}