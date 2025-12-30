using FocusFlow.Application.Common.Events;
using FocusFlow.Application.Interfaces;
using FocusFlow.Infrastructure.Data;
using FocusFlow.Infrastructure.Identity;
using FocusFlow.Infrastructure.Identity.Extensions;
using FocusFlow.Infrastructure.Repositories;
using FocusFlow.Infrastructure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Database Context
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

		services.AddDbContext<FocusFlowDbContext>(options =>
		{
			// Use PostgreSQL
			options.UseNpgsql(connectionString, npgsqlOptions =>
			{
				npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application);
			});

			// Development settings
#if DEBUG
			options.EnableSensitiveDataLogging();
			options.EnableDetailedErrors();
#endif
		});

		// ASP.NET Core Identity (moved to extension)
		services.AddIdentityServices();

		// Repositories
		services.AddScoped<IProjectRepository, ProjectRepository>();
		services.AddScoped<ITaskRepository, TaskRepository>();
		services.AddScoped<IUnitOfWork, UnitOfWork>();
		services.AddScoped<DatabaseSeeder>();

		// Register event publisher
		services.AddScoped<IEventPublisher, SignalREventPublisher>();

		return services;
	}
}