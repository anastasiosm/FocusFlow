using FluentAssertions;
using FocusFlow.Domain.Entities;
using FocusFlow.Infrastructure.Data;
using FocusFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Tests.Repositories;

public class ProjectRepositoryTests : IDisposable
{
	private readonly FocusFlowDbContext _context;
	private readonly ProjectRepository _repository;

	public ProjectRepositoryTests()
	{
		var options = new DbContextOptionsBuilder<FocusFlowDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new FocusFlowDbContext(options);
		_repository = new ProjectRepository(_context);
	}

	[Fact]
	public async Task GetByOwnerIdWithTasksAsync_ShouldReturnProjectsWithTasks()
	{
		// Arrange
		var userId = "owner123";
		var project1 = new Project("Project 1", null, userId);
		var project2 = new Project("Project 2", null, userId);
		var otherUserProject = new Project("Other Project", null, "otherUser");

		await _context.Projects.AddRangeAsync(project1, project2, otherUserProject);
		await _context.SaveChangesAsync();

		// Add tasks
		var task1 = new ProjectTask("Task 1", null, project1.Id);
		var task2 = new ProjectTask("Task 2", null, project1.Id);
		var task3 = new ProjectTask("Task 3", null, project2.Id);

		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.GetByOwnerIdWithTasksAsync(userId);

		// Assert
		result.Should().HaveCount(2);
		result.Should().AllSatisfy(p => p.OwnerId.Should().Be(userId));
		
		var proj1 = result.First(p => p.Id == project1.Id);
		proj1.Tasks.Should().HaveCount(2);
		
		var proj2 = result.First(p => p.Id == project2.Id);
		proj2.Tasks.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetByOwnerIdWithTasksAsync_WithNoProjects_ShouldReturnEmptyList()
	{
		// Arrange & Act
		var result = await _repository.GetByOwnerIdWithTasksAsync("nonexistent-user");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetByOwnerIdWithTasksAsync_ShouldOrderByName()
	{
		// Arrange
		var userId = "user123";
		var projectC = new Project("C Project", null, userId);
		var projectA = new Project("A Project", null, userId);
		var projectB = new Project("B Project", null, userId);

		await _context.Projects.AddRangeAsync(projectC, projectA, projectB);
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.GetByOwnerIdWithTasksAsync(userId);

		// Assert
		result.Should().HaveCount(3);
		result[0].Name.Should().Be("A Project");
		result[1].Name.Should().Be("B Project");
		result[2].Name.Should().Be("C Project");
	}

	[Fact]
	public async Task GetByOwnerIdWithTasksAsync_ShouldIncludeTasksInSingleQuery()
	{
		// Arrange
		var userId = "user456";
		var project = new Project("Test Project", null, userId);
		
		await _context.Projects.AddAsync(project);
		await _context.SaveChangesAsync();

		var task1 = new ProjectTask("Task 1", null, project.Id);
		var task2 = new ProjectTask("Task 2", null, project.Id);
		
		await _context.Tasks.AddRangeAsync(task1, task2);
		await _context.SaveChangesAsync();

		// Clear change tracker to ensure fresh query
		_context.ChangeTracker.Clear();

		// Act
		var result = await _repository.GetByOwnerIdWithTasksAsync(userId);

		// Assert
		result.Should().HaveCount(1);
		result[0].Tasks.Should().HaveCount(2);
		// Verify tasks are loaded (not lazy loaded)
		result[0].Tasks.Should().NotBeNull();
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}
}
