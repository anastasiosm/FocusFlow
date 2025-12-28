using FocusFlow.BlazorApp.Features.Tasks.Shared.Services;

namespace FocusFlow.BlazorApp.Features.Tasks;

/// <summary>
/// Feature registration for Tasks functionality
/// Registers all services, validators, and dependencies for the Tasks feature
/// </summary>
public static class TasksFeature
{
	/// <summary>
	/// Registers the application's task-related services with the specified dependency injection container.
	/// </summary>
	/// <remarks>Call this method during application startup to ensure that all required task services are available
	/// for dependency injection.</remarks>
	/// <param name="services">The service collection to which the task-related services will be added. Cannot be null.</param>
	public static IServiceCollection AddTasksFeature(this IServiceCollection services)
	{
		// Note: ITasksApi is registered via Refit in Program.cs
		// No additional services needed for now
		
		return services;
	}
}
