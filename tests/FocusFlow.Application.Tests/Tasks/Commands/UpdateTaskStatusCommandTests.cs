using FluentAssertions;
using FocusFlow.Application.Tasks.Commands;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;
using Moq;
using TaskStatus = FocusFlow.Domain.Enums.TaskStatus;

namespace FocusFlow.Application.Tests.Tasks.Commands;

public class UpdateTaskStatusCommandTests : TestBase
{
	private readonly UpdateTaskStatusCommandHandler _handler;

	public UpdateTaskStatusCommandTests()
	{
		_handler = new UpdateTaskStatusCommandHandler(
			MockTaskRepository.Object,
			MockUnitOfWork.Object,
			Mapper);
	}

	[Fact]
	public async Task Handle_WithExistingTask_ShouldUpdateStatus()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new UpdateTaskStatusCommand(taskId, TaskStatus.InProgress);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Status.Should().Be(TaskStatus.InProgress);
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

		var command = new UpdateTaskStatusCommand(taskId, TaskStatus.Done);

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

	[Fact]
	public async Task Handle_SettingStatusToDone_ShouldSetCompletedAt()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new UpdateTaskStatusCommand(taskId, TaskStatus.Done);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(TaskStatus.Done);
		result.CompletedAt.Should().NotBeNull();
		result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Handle_ReopeningCompletedTask_ShouldThrowBusinessRuleException()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		task.SetStatus(TaskStatus.Done); // Mark as completed

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new UpdateTaskStatusCommand(taskId, TaskStatus.Todo);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowBusinessRuleException>()
			.WithMessage("*Cannot reopen a completed task*");

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Theory]
	[InlineData(TaskStatus.Todo, TaskStatus.InProgress)]
	[InlineData(TaskStatus.InProgress, TaskStatus.Done)]
	[InlineData(TaskStatus.Todo, TaskStatus.Done)]
	public async Task Handle_WithValidStatusTransition_ShouldSucceed(
		TaskStatus initialStatus,
		TaskStatus newStatus)
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		if (initialStatus != TaskStatus.Todo)
			task.SetStatus(initialStatus);

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new UpdateTaskStatusCommand(taskId, newStatus);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(newStatus);
		VerifyUnitOfWorkSaveChanges(Times.Once());
	}
}