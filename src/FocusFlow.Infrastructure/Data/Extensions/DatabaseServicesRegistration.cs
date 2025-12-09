using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocusFlow.Infrastructure.Data.Extensions;

/// <summary>
/// Provides extension methods for registering and managing database services, including applying pending migrations to
/// the application's database.
/// </summary>
/// <remarks>This static class contains methods intended to be used during application startup or initialization
/// to ensure that the database schema is up to date. Methods in this class typically operate on services registered in
/// the application's dependency injection container.</remarks>
public static class DatabaseServicesRegistration
{
	public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
	{
		var dbContext = serviceProvider.GetRequiredService<FocusFlowDbContext>();
		var logger = serviceProvider.GetRequiredService<ILogger<FocusFlowDbContext>>();

		try
		{
			await dbContext.Database.MigrateAsync();
			logger.LogInformation("Database migrations applied successfully.");
		}
		catch (Exception e)
		{
			logger.LogError(e, "An error occurred while applying database migrations.");
			throw;
		}
	}
}

