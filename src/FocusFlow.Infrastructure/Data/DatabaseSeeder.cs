using FocusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FocusFlow.Infrastructure.Data;

public class DatabaseSeeder
{
	private readonly FocusFlowDbContext _context;
	private readonly ILogger<DatabaseSeeder> _logger;

	public DatabaseSeeder(FocusFlowDbContext context, ILogger<DatabaseSeeder> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task SeedAsync()
	{
		// Abort if there is already data
		if (await _context.Projects.AnyAsync())
		{
			_logger.LogInformation("Database already contains data; skipping seeding.");
			return;
		}

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
}