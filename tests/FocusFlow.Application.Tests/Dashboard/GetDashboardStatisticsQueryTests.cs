using FluentAssertions;
using FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using Moq;

namespace FocusFlow.Application.Tests.Dashboard;

public class GetDashboardStatisticsQueryTests : TestBase
{
	private readonly GetDashboardStatisticsQueryHandler _handler;

	public GetDashboardStatisticsQueryTests()
	{
		_handler = new GetDashboardStatisticsQueryHandler(
			MockProjectRepository.Object);
	}

	[Fact]
	public async Task Handle_WithProjects_ShouldReturnStatistics()
	{
		// Arrange
		var userId = "user123";
		var project1 = new Project("Project 1", "Description 1", userId);
		var project2 = new Project("Project 2", null, userId);

		// Add tasks to project1
		var task1 = new ProjectTask("Task 1", "Description", project1.Id);
		var task2 = new ProjectTask("Task 2", null, project1.Id);
		
		project1.AddTask(task1);
		project1.AddTask(task2);
		
		// Mark task1 as done
		task1.SetStatus(ProjectTaskStatus.Done);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project1, project2 });

		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		
		var stat1 = result.First(s => s.ProjectName == "Project 1");
		stat1.TotalTasks.Should().Be(2);
		stat1.CompletedTasks.Should().Be(1);

		var stat2 = result.First(s => s.ProjectName == "Project 2");
		stat2.TotalTasks.Should().Be(0);
		stat2.CompletedTasks.Should().Be(0);

		MockProjectRepository.Verify(
			repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_WithNoProjects_ShouldReturnEmptyList()
	{
		// Arrange
		var userId = "user-no-projects";

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project>());

		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();

		MockProjectRepository.Verify(
			repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_WithAllTasksCompleted_ShouldReturn100Percent()
	{
		// Arrange
		var userId = "user123";
		var project = new Project("Complete Project", null, userId);

		var task1 = new ProjectTask("Task 1", null, project.Id);
		var task2 = new ProjectTask("Task 2", null, project.Id);
		project.AddTask(task1);
		project.AddTask(task2);
		
		task1.SetStatus(ProjectTaskStatus.Done);
		task2.SetStatus(ProjectTaskStatus.Done);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project });

		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result.First().TotalTasks.Should().Be(2);
		result.First().CompletedTasks.Should().Be(2);
	}

	[Fact]
	public async Task Handle_WithNoCompletedTasks_ShouldReturn0Percent()
	{
		// Arrange
		var userId = "user123";
		var project = new Project("Incomplete Project", null, userId);

		var task1 = new ProjectTask("Task 1", null, project.Id);
		var task2 = new ProjectTask("Task 2", null, project.Id);
		project.AddTask(task1);
		project.AddTask(task2);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project });

		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result.First().TotalTasks.Should().Be(2);
		result.First().CompletedTasks.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithMultipleProjectsVariousCompletions_ShouldReturnCorrectStatistics()
	{
		// Arrange
		var userId = "user123";
		
		// Project with 50% completion
		var project1 = new Project("Project Half Done", null, userId);
		var task1a = new ProjectTask("Task 1A", null, project1.Id);
		var task1b = new ProjectTask("Task 1B", null, project1.Id);
		project1.AddTask(task1a);
		project1.AddTask(task1b);
		task1a.SetStatus(ProjectTaskStatus.Done);

		// Project with 100% completion
		var project2 = new Project("Project Complete", null, userId);
		var task2a = new ProjectTask("Task 2A", null, project2.Id);
		project2.AddTask(task2a);
		task2a.SetStatus(ProjectTaskStatus.Done);

		// Project with 0% completion
		var project3 = new Project("Project Not Started", null, userId);
		var task3a = new ProjectTask("Task 3A", null, project3.Id);
		project3.AddTask(task3a);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project1, project2, project3 });

		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(3);
		
		var projectHalfDone = result.First(s => s.ProjectName == "Project Half Done");
		projectHalfDone.TotalTasks.Should().Be(2);
		projectHalfDone.CompletedTasks.Should().Be(1);
		
		var projectComplete = result.First(s => s.ProjectName == "Project Complete");
		projectComplete.TotalTasks.Should().Be(1);
		projectComplete.CompletedTasks.Should().Be(1);
		
		var projectNotStarted = result.First(s => s.ProjectName == "Project Not Started");
		projectNotStarted.TotalTasks.Should().Be(1);
		projectNotStarted.CompletedTasks.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithOverdueTasks_ShouldCountThem()
	{
		// Arrange
		var userId = "user123";
		var project = new Project("Project With Overdue", null, userId);

		// Create overdue task (past due date)
		var overdueTask = new ProjectTask("Overdue Task", null, project.Id, DateTime.UtcNow.AddDays(-5));
		project.AddTask(overdueTask);

		// Create future task
		var futureTask = new ProjectTask("Future Task", null, project.Id, DateTime.UtcNow.AddDays(5));
		project.AddTask(futureTask);

		MockProjectRepository
			.Setup(repo => repo.GetByOwnerIdWithTasksAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Project> { project });

		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result.First().TotalTasks.Should().Be(2);
		result.First().OverdueTasks.Should().Be(1);
	}
}
