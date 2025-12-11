using FluentAssertions;
using FocusFlow.Application.Features.Tasks.UpdateTaskStatus;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;
using Moq;

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

		var command = new UpdateTaskStatusCommand(taskId, ProjectTaskStatus.InProgress);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Status.Should().Be(ProjectTaskStatus.InProgress);
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

		var command = new UpdateTaskStatusCommand(taskId, ProjectTaskStatus.Done);

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

		var command = new UpdateTaskStatusCommand(taskId, ProjectTaskStatus.Done);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(ProjectTaskStatus.Done);
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

		task.SetStatus(ProjectTaskStatus.Done); // Mark as completed

		MockTaskRepository
			.Setup(repo => repo.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);

		var command = new UpdateTaskStatusCommand(taskId, ProjectTaskStatus.Todo);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowBusinessRuleException>()
			.WithMessage("*Cannot reopen a completed task*");

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Theory]
	[InlineData(ProjectTaskStatus.Todo, ProjectTaskStatus.InProgress)]
	[InlineData(ProjectTaskStatus.InProgress, ProjectTaskStatus.Done)]
	[InlineData(ProjectTaskStatus.Todo, ProjectTaskStatus.Done)]
	public async Task Handle_WithValidStatusTransition_ShouldSucceed(
		ProjectTaskStatus initialStatus,
		ProjectTaskStatus newStatus)
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new ProjectTask("Task", null, Guid.NewGuid());

		var idProperty = typeof(ProjectTask).GetProperty("Id");
		idProperty!.SetValue(task, taskId);

		if (initialStatus != ProjectTaskStatus.Todo)
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