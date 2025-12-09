using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocusFlow.Infrastructure.Data.Extensions;

public static class DatabaseServicesRegistration
{
	public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<FocusFlowDbContext>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<FocusFlowDbContext>>();

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

