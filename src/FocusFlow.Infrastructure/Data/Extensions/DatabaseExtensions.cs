using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocusFlow.Infrastructure.Data.Extensions;

/// <summary>
/// Provides extension methods for registering and managing database services, including applying pending migrations to
/// the application's database.
/// </summary>
public static class DatabaseExtensions
{
	public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
	{
		// create a new scope to retrieve scoped services
		// before resolving the database context.
		using var scope = serviceProvider.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<FocusFlowDbContext>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<FocusFlowDbContext>>();

		try
		{
			if (dbContext.Database.IsRelational())
			{
				await dbContext.Database.MigrateAsync();
				logger.LogInformation("Database migrations applied successfully.");
			}
		}
		catch (Exception e)
		{
			logger.LogError(e, "An error occurred while applying database migrations.");
			throw;
		}
	}

	/// <summary>
	/// Runs the <see cref="DatabaseSeeder"/> to populate initial data when database is empty.
	/// </summary>
	public static async Task SeedAsync(this IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<FocusFlowDbContext>>();
		try
		{
			var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
			await seeder.SeedAsync();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while seeding the database.");
			throw;
		}
	}
}

