using FocusFlow.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Infrastructure.Identity.Extensions;

public static class IdentityServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityServices(this IServiceCollection services)
	{
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

		return services;
	}
}
