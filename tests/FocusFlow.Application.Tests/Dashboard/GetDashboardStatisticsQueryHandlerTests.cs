using FluentAssertions;
using FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using Moq;

namespace FocusFlow.Application.Tests.Dashboard;

public class GetDashboardStatisticsQueryHandlerTests : TestBase
{
	[Fact]
	public async Task Handle_WithMultipleProjects_ShouldReturnCorrectStatistics()
	{
		// Arrange
		var userId = "user123";
		var project1Id = Guid.NewGuid();
		var project2Id = Guid.NewGuid();

		var project1 = new Project("Project 1", "Description 1", userId);
		var project2 = new Project("Project 2", "Description 2", userId);

		// Set IDs using reflection
		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project1, project1Id);
		idProperty!.SetValue(project2, project2Id);

		// Project 1: 3 tasks (1 completed, 1 overdue)
		var task1 = new ProjectTask("Task 1", null, project1Id, DateTime.UtcNow.AddDays(-2));
		task1.SetStatus(ProjectTaskStatus.Done);
		
		var task2 = new ProjectTask("Task 2", null, project1Id, DateTime.UtcNow.AddDays(-1)); // Overdue
		
		var task3 = new ProjectTask("Task 3", null, project1Id, DateTime.UtcNow.AddDays(2)); // Not overdue
		
		project1.AddTask(task1);
		project1.AddTask(task2);
		project1.AddTask(task3);

		// Project 2: 2 tasks (2 completed, 0 overdue)
		var task4 = new ProjectTask("Task 4", null, project2Id);
		task4.SetStatus(ProjectTaskStatus.Done);
		
		var task5 = new ProjectTask("Task 5", null, project2Id);
		task5.SetStatus(ProjectTaskStatus.Done);
		
		project2.AddTask(task4);
		project2.AddTask(task5);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project1, project2 });

		var handler = new GetDashboardStatisticsQueryHandler(MockProjectRepository.Object);
		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);

		var stats1 = result.First(s => s.ProjectId == project1Id);
		stats1.ProjectName.Should().Be("Project 1");
		stats1.TotalTasks.Should().Be(3);
		stats1.CompletedTasks.Should().Be(1);
		stats1.OverdueTasks.Should().Be(1);

		var stats2 = result.First(s => s.ProjectId == project2Id);
		stats2.ProjectName.Should().Be("Project 2");
		stats2.TotalTasks.Should().Be(2);
		stats2.CompletedTasks.Should().Be(2);
		stats2.OverdueTasks.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithNoProjects_ShouldReturnEmptyList()
	{
		// Arrange
		var userId = "user-no-projects";

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project>());

		var handler = new GetDashboardStatisticsQueryHandler(MockProjectRepository.Object);
		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithProjectsWithoutTasks_ShouldReturnZeroStatistics()
	{
		// Arrange
		var userId = "user456";
		var projectId = Guid.NewGuid();
		var project = new Project("Empty Project", null, userId);

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project });

		var handler = new GetDashboardStatisticsQueryHandler(MockProjectRepository.Object);
		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result.First().TotalTasks.Should().Be(0);
		result.First().CompletedTasks.Should().Be(0);
		result.First().OverdueTasks.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithMixedTaskStatuses_ShouldCalculateCorrectly()
	{
		// Arrange
		var userId = "user789";
		var projectId = Guid.NewGuid();
		var project = new Project("Mixed Project", null, userId);

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project, projectId);

		// Create tasks with different statuses
		var completedTask = new ProjectTask("Completed", null, projectId, DateTime.UtcNow.AddDays(-5));
		completedTask.SetStatus(ProjectTaskStatus.Done);

		var overdueTask = new ProjectTask("Overdue", null, projectId, DateTime.UtcNow.AddDays(-3));

		var inProgressTask = new ProjectTask("In Progress", null, projectId, DateTime.UtcNow.AddDays(5));
		inProgressTask.SetStatus(ProjectTaskStatus.InProgress);

		var todoTask = new ProjectTask("Todo", null, projectId, null);

		project.AddTask(completedTask);
		project.AddTask(overdueTask);
		project.AddTask(inProgressTask);
		project.AddTask(todoTask);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project });

		var handler = new GetDashboardStatisticsQueryHandler(MockProjectRepository.Object);
		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		var stats = result.First();
		stats.TotalTasks.Should().Be(4);
		stats.CompletedTasks.Should().Be(1);
		stats.OverdueTasks.Should().Be(1); // Only the one with past due date and not done
	}
}
