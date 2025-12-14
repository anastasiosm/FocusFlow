using FocusFlow.Domain.Entities;
using FocusFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FocusFlow.Infrastructure.Data;

public class DatabaseSeeder
{
	private readonly FocusFlowDbContext _context;
	private readonly ILogger<DatabaseSeeder> _logger;
	private readonly UserManager<ApplicationUser>? _userManager;
	private readonly IConfiguration _configuration;

	public DatabaseSeeder(FocusFlowDbContext context, ILogger<DatabaseSeeder> logger, IConfiguration configuration, UserManager<ApplicationUser>? userManager = null)
	{
		_context = context;
		_logger = logger;
		_configuration = configuration;
		_userManager = userManager;
	}

	public async Task SeedAsync()
	{
		// Abort if there is already data
		if (await _context.Projects.AnyAsync())
		{
			_logger.LogInformation("Database already contains data; skipping seeding.");
		}
		else
		{
			// Create sample projects and tasks
			var project1 = new Project("Seeded Project 1", "This is a seeded project", "seed-user-1");
			project1.AddTask(new ProjectTask("Initial task A", "First seeded task", project1.Id));
			project1.AddTask(new ProjectTask("Initial task B", null, project1.Id));

			var project2 = new Project("Seeded Project 2", null, "seed-user-2");
			project2.AddTask(new ProjectTask("Initial task C", "Another seeded task", project2.Id));

			_context.Projects.AddRange(project1, project2);

			await _context.SaveChangesAsync();

			_logger.LogInformation("Database seeding completed: {ProjectCount} projects added.", 2);
		}

		// Optionally create a test user when configured (useful for E2E runs)
		try
		{
			var testEmail = _configuration["TestUser:Email"] ?? Environment.GetEnvironmentVariable("TEST_USER_EMAIL");
			var testPassword = _configuration["TestUser:Password"] ?? Environment.GetEnvironmentVariable("TEST_USER_PASSWORD");

			if (!string.IsNullOrWhiteSpace(testEmail) && !string.IsNullOrWhiteSpace(testPassword) && _userManager != null)
			{
				var existing = await _userManager.FindByEmailAsync(testEmail);
				if (existing == null)
				{
					var user = new ApplicationUser
					{
						UserName = testEmail,
						Email = testEmail,
						FirstName = "E2E",
						LastName = "User",
						EmailConfirmed = true
					};

					var result = await _userManager.CreateAsync(user, testPassword);
					if (result.Succeeded)
					{
						_logger.LogInformation("Created test user {Email} for E2E tests.", testEmail);
					}
					else
					{
						_logger.LogWarning("Failed to create test user {Email}: {Errors}", testEmail, string.Join(';', result.Errors.Select(e => e.Description)));
					}
				}
				else
				{
					_logger.LogInformation("Test user {Email} already exists.", testEmail);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to ensure test user creation.");
		}
	}
}