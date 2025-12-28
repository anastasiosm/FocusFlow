using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.BlazorApp.Features.Home;

/// <summary>
/// Home feature registration
/// </summary>
public static class HomeFeature
{
    public static IServiceCollection AddHomeFeature(this IServiceCollection services)
    {
        // Add Home-specific services here if needed
        return services;
    }
}