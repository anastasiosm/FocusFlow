using FocusFlow.Application.Interfaces;
using FocusFlow.Infrastructure.Data;
using FocusFlow.Infrastructure.Identity;
using FocusFlow.Infrastructure.Repositories;
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

		// ASP.NET Core Identity
		services.AddIdentityCore<ApplicationUser>(options =>
		{
			// Password settings
			options.Password.RequireDigit = true;
			options.Password.RequiredLength = 8;
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequireUppercase = true;
			options.Password.RequireLowercase = true;

			// Lockout settings
			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			options.Lockout.MaxFailedAccessAttempts = 5;
			options.Lockout.AllowedForNewUsers = true;

			// User settings
			options.User.RequireUniqueEmail = true;
			options.SignIn.RequireConfirmedEmail = false; // Set to true in production
		})
		.AddEntityFrameworkStores<FocusFlowDbContext>();

		// Repositories
		services.AddScoped<IProjectRepository, ProjectRepository>();
		services.AddScoped<ITaskRepository, TaskRepository>();
		services.AddScoped<IUnitOfWork, UnitOfWork>();
		services.AddScoped<DatabaseSeeder>();

		return services;
	}
}