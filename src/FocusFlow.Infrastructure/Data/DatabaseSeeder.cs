//using Bogus;
//using FocusFlow.Domain.Entities;
//using FocusFlow.Domain.Enums;
//using FocusFlow.Infrastructure.Data;
//using FocusFlow.Infrastructure.Identity;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace FocusFlow.Infrastructure.Data;

///// <summary>
///// Seeds initial data for development/testing using Bogus
///// </summary>
//public static class DatabaseSeeder
//{
//	public static async Task SeedAsync(IServiceProvider serviceProvider)
//	{
//		using var scope = serviceProvider.CreateScope();
//		var context = scope.ServiceProvider.GetRequiredService<FocusFlowDbContext>();
//		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//		var logger = scope.ServiceProvider.GetRequiredService<ILogger<FocusFlowDbContext>>();

//		try
//		{
//			// Seed users
//			if (!await context.Users.AnyAsync())
//			{
//				await SeedUsersAsync(userManager, logger);
//			}

//			// Seed projects and tasks
//			if (!await context.Projects.AnyAsync())
//			{
//				await SeedProjectsAsync(context, userManager, logger);
//			}

//			logger.LogInformation("Database seeding completed successfully");
//		}
//		catch (Exception ex)
//		{
//			logger.LogError(ex, "An error occurred while seeding the database");
//			throw;
//		}
//	}

//	private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
//	{
//		// Create demo user
//		var demoUser = new ApplicationUser
//		{
//			UserName = "demo@focusflow.com",
//			Email = "demo@focusflow.com",
//			FirstName = "Demo",
//			LastName = "User",
//			EmailConfirmed = true
//		};

//		var result = await userManager.CreateAsync(demoUser, "Demo@123");

//		if (result.Succeeded)
//		{
//			logger.LogInformation("Demo user created: {Email}", demoUser.Email);
//		}
//		else
//		{
//			logger.LogWarning("Failed to create demo user: {Errors}",
//				string.Join(", ", result.Errors.Select(e => e.Description)));
//			return;
//		}

//		// Create additional fake users with Bogus
//		var userFaker = new Faker<ApplicationUser>()
//			.RuleFor(u => u.FirstName, f => f.Name.FirstName())
//			.RuleFor(u => u.LastName, f => f.Name.LastName())
//			.RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
//			.RuleFor(u => u.UserName, (f, u) => u.Email)
//			.RuleFor(u => u.EmailConfirmed, f => true);

//		var fakeUsers = userFaker.Generate(3);

//		foreach (var user in fakeUsers)
//		{
//			var createResult = await userManager.CreateAsync(user, "User@123");
//			if (createResult.Succeeded)
//			{
//				logger.LogInformation("Fake user created: {Email}", user.Email);
//			}
//		}
//	}

//	private static async Task SeedProjectsAsync(
//		FocusFlowDbContext context,
//		UserManager<ApplicationUser> userManager,
//		ILogger logger)
//	{
//		var demoUser = await userManager.FindByEmailAsync("demo@focusflow.com");
//		if (demoUser == null)
//		{
//			logger.LogWarning("Demo user not found, skipping project seeding");
//			return;
//		}

//		var allUsers = await userManager.Users.ToListAsync();
//		var faker = new Faker();

//		// Seed realistic development project
//		await SeedDevelopmentProjectAsync(context, demoUser.Id, allUsers, logger);

//		// Seed multiple fake projects
//		await SeedFakeProjectsAsync(context, demoUser.Id, allUsers, faker, logger);
//	}

//	private static async Task SeedDevelopmentProjectAsync(
//		FocusFlowDbContext context,
//		string ownerId,
//		List<ApplicationUser> allUsers,
//		ILogger logger)
//	{
//		var project = new Project(
//			"FocusFlow Development",
//			"Building the FocusFlow task management system with Clean Architecture, CQRS, and modern best practices",
//			ownerId
//		);

//		var developmentTasks = new List<(string Title, string Description, int DueDays, Priority Priority, TaskStatus Status)>
//		{
//			("Setup Database", "Configure PostgreSQL with EF Core migrations and seed data", -5, Priority.High, TaskStatus.Done),
//			("Implement Domain Layer", "Create entities with business logic and validation rules", -3, Priority.High, TaskStatus.Done),
//			("Build Application Layer", "Setup MediatR, AutoMapper, and FluentValidation", -1, Priority.High, TaskStatus.Done),
//			("Configure Infrastructure", "Implement repositories and Unit of Work pattern", 0, Priority.High, TaskStatus.InProgress),
//			("Create Web API", "Setup controllers with JWT authentication and Swagger", 2, Priority.Critical, TaskStatus.InProgress),
//			("Build Blazor UI", "Create responsive UI with MudBlazor components", 7, Priority.Medium, TaskStatus.Todo),
//			("Add SignalR Support", "Implement real-time task updates", 10, Priority.Low, TaskStatus.Todo),
//			("Write Unit Tests", "Achieve 90%+ code coverage across all layers", 5, Priority.High, TaskStatus.InProgress),
//			("Setup Docker", "Create Dockerfiles and docker-compose configuration", 12, Priority.Medium, TaskStatus.Todo),
//			("Write Documentation", "Complete README, API docs, and architecture diagrams", 15, Priority.Low, TaskStatus.Todo)
//		};

//		foreach (var (title, description, dueDays, priority, status) in developmentTasks)
//		{
//			var task = new ProjectTask(
//				title,
//				description,
//				project.Id,
//				DateTime.UtcNow.AddDays(dueDays),
//				priority,
//				dueDays <= 0 ? ownerId : (new Faker().Random.Bool(0.6f) ? allUsers[new Faker().Random.Int(0, allUsers.Count - 1)].Id : null)
//			);

//			if (status != TaskStatus.Todo)
//				task.SetStatus(status);

//			project.AddTask(task);
//		}

//		context.Projects.Add(project);
//		await context.SaveChangesAsync();

//		logger.LogInformation("Seeded development project with {TaskCount} tasks", developmentTasks.Count);
//	}

//	private static async Task SeedFakeProjectsAsync(
//		FocusFlowDbContext context,
//		string ownerId,
//		List<ApplicationUser> allUsers,
//		Faker faker,
//		ILogger logger)
//	{
//		var projectCategories = new[]
//		{
//			("Work", new[] { "Meeting", "Report", "Analysis", "Review", "Planning" }),
//			("Personal", new[] { "Shopping", "Exercise", "Reading", "Learning", "Health" }),
//			("Home", new[] { "Cleaning", "Repairs", "Organizing", "Maintenance", "Renovation" }),
//			("Finance", new[] { "Budget", "Invoice", "Tax", "Investment", "Payment" }),
//			("Creative", new[] { "Design", "Writing", "Photography", "Music", "Art" })
//		};

//		var projects = new List<Project>();

//		// Create 8-12 random projects
//		var projectCount = faker.Random.Int(8, 12);

//		for (int i = 0; i < projectCount; i++)
//		{
//			var category = faker.PickRandom(projectCategories);
//			var categoryName = category.Item1;

//			var project = new Project(
//				$"{faker.Commerce.ProductName()} - {categoryName}",
//				faker.Lorem.Sentence(faker.Random.Int(5, 15)),
//				faker.Random.Bool(0.8f) ? ownerId : allUsers[faker.Random.Int(0, allUsers.Count - 1)].Id
//			);

//			// Add 3-12 tasks per project
//			var taskCount = faker.Random.Int(3, 12);
//			var taskKeywords = category.Item2;

//			for (int j = 0; j < taskCount; j++)
//			{
//				var daysOffset = faker.Random.Int(-10, 30);
//				var isOverdue = daysOffset < 0;
//				var priority = faker.PickRandom<Priority>();

//				// Determine status based on due date and randomness
//				TaskStatus status;
//				if (isOverdue)
//				{
//					// Overdue tasks: 60% still todo/in-progress, 40% done
//					status = faker.Random.Bool(0.6f)
//						? faker.PickRandom(TaskStatus.Todo, TaskStatus.InProgress)
//						: TaskStatus.Done;
//				}
//				else
//				{
//					// Future tasks: 10% done, 30% in-progress, 60% todo
//					var rand = faker.Random.Float();
//					status = rand < 0.1f ? TaskStatus.Done
//						: rand < 0.4f ? TaskStatus.InProgress
//						: TaskStatus.Todo;
//				}

//				var task = new ProjectTask(
//					$"{faker.PickRandom(taskKeywords)} {faker.Hacker.Verb()} {faker.Commerce.ProductMaterial()}",
//					faker.Random.Bool(0.7f) ? faker.Lorem.Sentence(faker.Random.Int(5, 20)) : null,
//					project.Id,
//					faker.Random.Bool(0.85f) ? DateTime.UtcNow.AddDays(daysOffset) : null,
//					priority,
//					faker.Random.Bool(0.5f) ? allUsers[faker.Random.Int(0, allUsers.Count - 1)].Id : null
//				);

//				if (status != TaskStatus.Todo)
//					task.SetStatus(status);

//				project.AddTask(task);
//			}

//			projects.Add(project);
//		}

//		context.Projects.AddRange(projects);
//		await context.SaveChangesAsync();

//		var totalTasks = projects.Sum(p => p.Tasks.Count);
//		var completedTasks = projects.SelectMany(p => p.Tasks).Count(t => t.Status == TaskStatus.Done);
//		var overdueTasks = projects.SelectMany(p => p.Tasks).Count(t => t.IsOverdue());

//		logger.LogInformation(
//			"Seeded {ProjectCount} fake projects with {TaskCount} tasks ({CompletedCount} completed, {OverdueCount} overdue)",
//			projects.Count,
//			totalTasks,
//			completedTasks,
//			overdueTasks);
//	}
//}