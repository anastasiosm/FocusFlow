using FluentAssertions;
using FocusFlow.Application.Tasks.Queries;
using FocusFlow.Domain.Entities;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Queries;

public class GetTasksByProjectQueryTests : TestBase
{
	private readonly GetTasksByProjectQueryHandler _handler;

	public GetTasksByProjectQueryTests()
	{
		_handler = new GetTasksByProjectQueryHandler(MockTaskRepository.Object, Mapper);
	}

	[Fact]
	public async Task Handle_WithTasksInProject_ShouldReturnAllTasks()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var tasks = new List<ProjectTask>
		{
			new("Task 1", "Desc 1", projectId),
			new("Task 2", "Desc 2", projectId)
		};

		MockTaskRepository
			.Setup(repo => repo.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(tasks);

		var query = new GetTasksByProjectQuery(projectId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(t => t.Title == "Task 1");
		result.Should().Contain(t => t.Title == "Task 2");
		result.Should().AllSatisfy(t => t.ProjectId.Should().Be(projectId));
	}

	[Fact]
	public async Task Handle_WithNoTasks_ShouldReturnEmptyList()
	{
		// Arrange
		var projectId = Guid.NewGuid();

		MockTaskRepository
			.Setup(repo => repo.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<ProjectTask>());

		var query = new GetTasksByProjectQuery(projectId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
