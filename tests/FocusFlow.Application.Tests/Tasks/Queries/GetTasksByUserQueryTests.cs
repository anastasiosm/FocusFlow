using FluentAssertions;
using FocusFlow.Application.Tasks.Queries;
using FocusFlow.Domain.Entities;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Queries;

public class GetTasksByUserQueryTests : TestBase
{
	private readonly GetTasksByUserQueryHandler _handler;

	public GetTasksByUserQueryTests()
	{
		_handler = new GetTasksByUserQueryHandler(MockTaskRepository.Object, Mapper);
	}

	[Fact]
	public async Task Handle_WithAssignedTasks_ShouldReturnUserTasks()
	{
		// Arrange
		var userId = "test-user";
		var tasks = new List<ProjectTask>
		{
			new("Task 1", "Desc 1", Guid.NewGuid(), assignedUserId: userId),
			new("Task 2", "Desc 2", Guid.NewGuid(), assignedUserId: userId)
		};

		MockTaskRepository
			.Setup(repo => repo.GetByAssignedUserIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByUserQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(t => t.Title == "Task 1");
		result.Should().Contain(t => t.Title == "Task 2");
		result.Should().AllSatisfy(t => t.AssignedUserId.Should().Be(userId));
	}

	[Fact]
	public async Task Handle_WithNoAssignedTasks_ShouldReturnEmptyList()
	{
		// Arrange
		var userId = "test-user-no-tasks";

		MockTaskRepository
			.Setup(repo => repo.GetByAssignedUserIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<ProjectTask>());

		var query = new GetTasksByUserQuery(userId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
