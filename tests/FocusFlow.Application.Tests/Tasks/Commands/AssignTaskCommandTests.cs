using AutoMapper;
using FluentAssertions;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using FocusFlow.Application.Tasks.Commands;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Commands;

public class AssignTaskCommandTests : TestBase
{
	private readonly AssignTaskCommandHandler _handler;

	public AssignTaskCommandTests()
	{
		_handler = new AssignTaskCommandHandler(
			MockTaskRepository.Object,
			MockUnitOfWork.Object,
			Mapper);
	}

	[Fact]
	public async Task Handle_WithValidCommand_ShouldAssignUserToTask()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var userId = "test-user-id";
		var task = new ProjectTask("Test Task", "Test Description", projectId);

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new AssignTaskCommand(taskId, userId);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.AssignedUserId.Should().Be(userId);
		result.Id.Should().Be(taskId);

		MockTaskRepository.Verify(
			repo => repo.UpdateAsync(task, It.IsAny<CancellationToken>()),
			Times.Once);

		VerifyUnitOfWorkSaveChanges(Times.Once());
	}

	[Fact]
	public async Task Handle_WithNonExistingTask_ShouldThrowNotFoundException()
	{
		// Arrange
		var taskId = Guid.NewGuid();

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((ProjectTask?)null);

		var command = new AssignTaskCommand(taskId, "test-user-id");

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowNotFoundException>()
			.WithMessage("*Task*not found*");

		MockTaskRepository.Verify(
			repo => repo.UpdateAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()),
			Times.Never);

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task Handle_WithEmptyUserId_ShouldThrowValidationException(string invalidUserId)
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var projectId = Guid.NewGuid();
		var task = new ProjectTask("Task", null, projectId);

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new AssignTaskCommand(taskId, invalidUserId);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowValidationException>()
			.WithMessage("*User ID cannot be empty*");

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}
}
