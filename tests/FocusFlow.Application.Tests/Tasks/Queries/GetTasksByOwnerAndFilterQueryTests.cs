using FluentAssertions;
using FocusFlow.Application.Features.Tasks.GetTasksByOwnerAndFilter;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Queries;

public class GetTasksByOwnerAndFilterQueryTests : TestBase
{
    [Fact]
    public async Task GetTasksByOwnerAndFilterQuery_WhenCalled_ShouldReturnOwnedTasks()
    {
        // Arrange
        var ownerId = "user-123";
        var projectId = Guid.NewGuid();
        var tasks = new List<ProjectTask>
        {
            new ProjectTask("Task 1", "Description 1", projectId),
            new ProjectTask("Task 2", "Description 2", projectId)
        };

        MockTaskRepository
            .Setup(repo => repo.GetByOwnerWithFiltersAsync(
                ownerId,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new GetTasksByOwnerAndFilterQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTasksByOwnerAndFilterQuery(ownerId, null, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.ProjectId.Should().Be(projectId));
    }

    [Fact]
    public async Task GetTasksByOwnerAndFilterQuery_WithStatusFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var ownerId = "user-123";
        var projectId = Guid.NewGuid();
		var desiredStatus = ProjectTaskStatus.InProgress;

        var task = new ProjectTask("Task 1", "Description 1", projectId);
        task.SetStatus(desiredStatus);
        
        var tasks = new List<ProjectTask> { task };

        MockTaskRepository
            .Setup(repo => repo.GetByOwnerWithFiltersAsync(
                ownerId,
                desiredStatus,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new GetTasksByOwnerAndFilterQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTasksByOwnerAndFilterQuery(ownerId, desiredStatus, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(desiredStatus);
    }

	[Fact]
    public async Task GetTasksByOwnerAndFilterQuery_WithPriorityFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var ownerId = "user-123";
        var projectId = Guid.NewGuid();
		var desiredPriority = Priority.High;

        var tasks = new List<ProjectTask>
        {
            new ProjectTask("Task 1", "Description 1", projectId, null, desiredPriority)
        };

        MockTaskRepository
            .Setup(repo => repo.GetByOwnerWithFiltersAsync(
                ownerId,
                null,
                desiredPriority,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new GetTasksByOwnerAndFilterQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTasksByOwnerAndFilterQuery(ownerId, null, desiredPriority, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Priority.Should().Be(desiredPriority);
    }

	[Fact]
    public async Task GetTasksByOwnerAndFilterQuery_WithOverdueFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var ownerId = "user-123";
        var projectId = Guid.NewGuid();
		var isOverdue = true;
        var pastDueDate = DateTime.UtcNow.AddDays(-1);

        var tasks = new List<ProjectTask>
        {
            new ProjectTask("Task 1", "Description 1", projectId, pastDueDate)
        };

        MockTaskRepository
            .Setup(repo => repo.GetByOwnerWithFiltersAsync(
                ownerId,
                null,
                null,
                isOverdue,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new GetTasksByOwnerAndFilterQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTasksByOwnerAndFilterQuery(ownerId, null, null, isOverdue);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().DueDate.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetTasksByOwnerAndFilterQuery_WithNoMatchingTasks_ShouldReturnEmptyList()
    {
        // Arrange
        var ownerId = "user-xyz";
        
        MockTaskRepository
            .Setup(repo => repo.GetByOwnerWithFiltersAsync(
                ownerId,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectTask>());

        var handler = new GetTasksByOwnerAndFilterQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTasksByOwnerAndFilterQuery(ownerId, null, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
