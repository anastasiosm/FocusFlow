using FocusFlow.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace FocusFlow.Integration.Tests;

/// <summary>
/// Base class for all integration tests, providing a shared WebApplicationFactory context.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<FocusFlowWebApplicationFactory>
{
    protected readonly FocusFlowWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected IntegrationTestBase(FocusFlowWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Clear the database before each test
        ClearDatabase();
    }

    private void ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FocusFlowDbContext>();
        
        // Use Database.EnsureDeleted and EnsureCreated to reset the in-memory database
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Clear the auth header for a clean slate
        _client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task AuthenticateAsync(string username = "testuser", string email = "test@example.com", string password = "Password123!")
    {
        // 1. Register via API
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new 
        { 
            Email = email, 
            Password = password,
            FirstName = "Test",
            LastName = username
        });

        // 2. Login to get token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new 
        { 
            Email = email, 
            Password = password 
        });

        if (loginResponse.IsSuccessStatusCode)
        {
            var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            if (authResponse != null)
            {
                _client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", authResponse.Token);
            }
        }
    }

    // Helper class for deserializing login response
    private record AuthResponse(string Token, string UserName, string Email, DateTime Expiration);
}
