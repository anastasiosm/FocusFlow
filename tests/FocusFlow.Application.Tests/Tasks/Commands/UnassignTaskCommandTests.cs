using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.UnassignTask;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Commands;

public class UnassignTaskCommandTests
{
	private readonly Mock<ITaskRepository> _taskRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly UnassignTaskCommandHandler _handler;

	public UnassignTaskCommandTests()
	{
		_taskRepositoryMock = new Mock<ITaskRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_mapperMock = new Mock<IMapper>();
		_handler = new UnassignTaskCommandHandler(
			_taskRepositoryMock.Object,
			_unitOfWorkMock.Object,
			_mapperMock.Object);
	}

	[Fact]
	public async Task Handle_ShouldUnassignUserFromTask_WhenTaskExists()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var command = new UnassignTaskCommand(taskId);
		var task = new ProjectTask("Test Task", "Test Description", projectId, assignedUserId: "some-assigned-user-id");
		var taskDto = new TaskDto(taskId, "Test Task", "Test Description", null, ProjectTaskStatus.Todo, Priority.Medium, null, projectId, null, DateTime.UtcNow, DateTime.UtcNow);

		_taskRepositoryMock.Setup(x => x.GetByIdAsync(command.TaskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(task);
		_mapperMock.Setup(m => m.Map<TaskDto>(task)).Returns(taskDto);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		_taskRepositoryMock.Verify(x => x.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
		_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		Assert.Null(result.AssignedUserId);
	}

	[Fact]
	public async Task Handle_ShouldThrowFocusFlowNotFoundException_WhenTaskDoesNotExist()
	{
		// Arrange
		var command = new UnassignTaskCommand(Guid.NewGuid());

		_taskRepositoryMock.Setup(x => x.GetByIdAsync(command.TaskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((ProjectTask)null!);

		// Act & Assert
		await Assert.ThrowsAsync<FocusFlowNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
	}
}
