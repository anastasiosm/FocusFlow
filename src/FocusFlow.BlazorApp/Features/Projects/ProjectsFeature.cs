using Microsoft.Extensions.DependencyInjection;
using FocusFlow.BlazorApp.Features.Projects.Shared.Services;
using FocusFlow.BlazorApp.Features.Projects.Create.Validation;
using FocusFlow.BlazorApp.Features.Projects.Edit.Validation;

namespace FocusFlow.BlazorApp.Features.Projects;

/// <summary>
/// Feature registration for Projects functionality
/// Registers all services, validators, and dependencies for the Projects feature
/// </summary>
public static class ProjectsFeature
{
    /// <summary>
    /// Registers all Projects feature services with the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddProjectsFeature(this IServiceCollection services)
    {
        // Register Projects API service
        // Note: The actual Refit registration is done in Program.cs with the base URL
        
        // Register Validators
        services.AddScoped<ProjectCreateFormModelValidator>();
        services.AddScoped<ProjectUpdateFormModelValidator>();
        services.AddScoped<UpdateProjectDtoValidator>();

        return services;
    }
}