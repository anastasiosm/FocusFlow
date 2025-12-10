using FluentAssertions;
using FocusFlow.Application.Tasks.Queries;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Queries;

public class GetTaskByIdQueryTests : TestBase
{
    [Fact]
    public async Task GetTaskByIdQuery_WithExistingTask_ShouldReturnTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var task = new ProjectTask("Test Task", "Description", projectId);

        var idProperty = typeof(ProjectTask).GetProperty("Id");
        idProperty!.SetValue(task, taskId);

        MockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new GetTaskByIdQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTaskByIdQuery(taskId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(taskId);
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTaskByIdQuery_WithNonExistingTask_ShouldThrowNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        MockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectTask?)null);

        var handler = new GetTaskByIdQueryHandler(MockTaskRepository.Object, Mapper);
        var query = new GetTaskByIdQuery(taskId);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FocusFlowNotFoundException>()
            .WithMessage("*Task*not found*");
    }
}
