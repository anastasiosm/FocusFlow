using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.BlazorApp.Features.Dashboard;

/// <summary>
/// Dashboard feature registration
/// </summary>
public static class DashboardFeature
{
    public static IServiceCollection AddDashboardFeature(this IServiceCollection services)
    {
        // Add Dashboard-specific services here if needed
        return services;
    }
}