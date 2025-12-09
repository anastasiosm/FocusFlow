using FocusFlow.Application.Interfaces;
using FocusFlow.Infrastructure.Data;
using FocusFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Database context
		services.AddDbContext<FocusFlowDbContext>(options =>
			options.UseNpgsql(
				configuration.GetConnectionString("DefaultConnection"),
				npgsqlOptions =>
				{
					npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application);
					options.UseSnakeCaseNamingConvention();
				}));

		// Repositories
		services.AddScoped<IProjectRepository, ProjectRepository>();
		services.AddScoped<ITaskRepository, TaskRepository>();

		// Unit of Work
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		// Database seeder
		services.AddScoped<DatabaseSeeder>();

		return services;
	}
}