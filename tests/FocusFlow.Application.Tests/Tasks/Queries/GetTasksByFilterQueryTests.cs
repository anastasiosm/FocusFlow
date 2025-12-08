using FluentAssertions;
using FocusFlow.Application.Tasks.Queries;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using Moq;
using TaskStatus = FocusFlow.Domain.Enums.TaskStatus;

namespace FocusFlow.Application.Tests.Tasks.Queries;

public class GetTasksByFilterQueryTests : TestBase
{
	private readonly GetTasksByFilterQueryHandler _handler;

	public GetTasksByFilterQueryTests()
	{
		_handler = new GetTasksByFilterQueryHandler(MockTaskRepository.Object, Mapper);
	}

	[Fact]
	public async Task Handle_WithNoFilters_ShouldReturnAllTasks()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", "Desc 1", Guid.NewGuid(), DateTime.UtcNow.AddDays(1), Priority.High),
			new("Task 2", "Desc 2", Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), Priority.Medium),
			new("Task 3", "Desc 3", Guid.NewGuid(), DateTime.UtcNow.AddDays(2), Priority.Low),
			new("Task 4", "Desc 4", Guid.NewGuid(), null, Priority.High)
		};

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery();

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(4);
	}

	[Fact]
	public async Task Handle_WithStatusFilter_ShouldReturnFilteredTasks()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid()),
			new("Task 2", null, Guid.NewGuid()),
			new("Task 3", null, Guid.NewGuid())
		};
		tasks[0].SetStatus(TaskStatus.Todo);
		tasks[1].SetStatus(TaskStatus.InProgress);
		tasks[2].SetStatus(TaskStatus.Todo);

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(Status: TaskStatus.Todo);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().AllSatisfy(t => t.Status.Should().Be(TaskStatus.Todo));
	}

	[Fact]
	public async Task Handle_WithPriorityFilter_ShouldReturnFilteredTasks()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), priority: Priority.High),
			new("Task 2", null, Guid.NewGuid(), priority: Priority.Medium),
			new("Task 3", null, Guid.NewGuid(), priority: Priority.High)
		};

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(Priority: Priority.High);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().AllSatisfy(t => t.Priority.Should().Be(Priority.High));
	}

	[Fact]
	public async Task Handle_WithIsOverdueTrue_ShouldReturnOnlyOverdueTasks()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(1)),  // Future - not overdue
			new("Task 2", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), // Past - overdue
			new("Task 3", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(-2)), // Past - overdue
			new("Task 4", null, Guid.NewGuid(), null)                          // No due date - not overdue
		};

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(IsOverdue: true);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(t => t.Title == "Task 2");
		result.Should().Contain(t => t.Title == "Task 3");
	}

	[Fact]
	public async Task Handle_WithIsOverdueFalse_ShouldReturnNonOverdueTasks()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(1)),  // Future
			new("Task 2", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), // Overdue
			new("Task 3", null, Guid.NewGuid(), null)                          // No due date
		};

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(IsOverdue: false);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(t => t.Title == "Task 1");
		result.Should().Contain(t => t.Title == "Task 3");
	}

	[Fact]
	public async Task Handle_WithCompletedTask_ShouldNotBeOverdue()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)) // Past due date
		};
		tasks[0].SetStatus(TaskStatus.Done); // But completed

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(IsOverdue: true);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty(); // Completed tasks are not overdue
	}

	[Fact]
	public async Task Handle_WithMultipleFilters_ShouldApplyAllFilters()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), null, Priority.High),
			new("Task 2", null, Guid.NewGuid(), null, Priority.High),
			new("Task 3", null, Guid.NewGuid(), null, Priority.Medium)
		};
		tasks[0].SetStatus(TaskStatus.Todo);
		tasks[1].SetStatus(TaskStatus.InProgress);
		tasks[2].SetStatus(TaskStatus.Todo);

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(
			Status: TaskStatus.Todo,
			Priority: Priority.High);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result.First().Title.Should().Be("Task 1");
		result.First().Status.Should().Be(TaskStatus.Todo);
		result.First().Priority.Should().Be(Priority.High);
	}

	[Fact]
	public async Task Handle_WithAllFilters_ShouldApplyCorrectly()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), Priority.High),
			new("Task 2", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), Priority.High),
			new("Task 3", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(1), Priority.High)
		};
		tasks[0].SetStatus(TaskStatus.Todo);   // Overdue, High, Todo
		tasks[1].SetStatus(TaskStatus.InProgress); // Overdue, High, InProgress
		tasks[2].SetStatus(TaskStatus.Todo);   // Not overdue, High, Todo

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(
			Status: TaskStatus.Todo,
			Priority: Priority.High,
			IsOverdue: true);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result.First().Title.Should().Be("Task 1");
	}

	[Fact]
	public async Task Handle_WithNoMatchingTasks_ShouldReturnEmptyList()
	{
		// Arrange
		var tasks = new List<ProjectTask>
		{
			new("Task 1", null, Guid.NewGuid(), priority: Priority.Low)
		};

		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByFilterQuery(Priority: Priority.Critical);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithEmptyTaskList_ShouldReturnEmptyList()
	{
		// Arrange
		MockTaskRepository
			.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<ProjectTask>());

		var query = new GetTasksByFilterQuery(Status: TaskStatus.Todo);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
