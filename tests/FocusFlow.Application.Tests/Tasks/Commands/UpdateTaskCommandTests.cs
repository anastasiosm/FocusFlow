using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.UpdateTask;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Commands;

public class UpdateTaskCommandTests
{
	private readonly Mock<ITaskRepository> _taskRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly UpdateTaskCommandHandler _handler;

	public UpdateTaskCommandTests()
	{
		_taskRepositoryMock = new Mock<ITaskRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_mapperMock = new Mock<IMapper>();
		_handler = new UpdateTaskCommandHandler(
			_taskRepositoryMock.Object,
			_unitOfWorkMock.Object,
			_mapperMock.Object);
	}

	[Fact]
	public async Task Handle_ShouldUpdateTask_WhenTaskExists()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var initialTask = new ProjectTask("Initial Title", "Initial Description", projectId); // Id is generated here
		var taskId = initialTask.Id; // Use the generated Id

		var command = new UpdateTaskCommand(
			taskId,
			"Updated Title",
			"Updated Description",
			DateTime.UtcNow.AddDays(1),
			Priority.High,
			null);

		var updatedTaskDto = new TaskDto(
			taskId,
			command.Title,
			command.Description,
			command.DueDate,
			ProjectTaskStatus.Todo,
			command.Priority,
			null,
			projectId,
			null,
			DateTime.UtcNow,
			DateTime.UtcNow);

		_taskRepositoryMock.Setup(x => x.GetByIdAsync(command.TaskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(initialTask);
		_mapperMock.Setup(m => m.Map<TaskDto>(initialTask)).Returns(updatedTaskDto);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		_taskRepositoryMock.Verify(x => x.UpdateAsync(initialTask, It.IsAny<CancellationToken>()), Times.Once);
		_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

		Assert.Equal(command.Title, result.Title);
		Assert.Equal(command.Description, result.Description);
		Assert.Equal(command.DueDate?.Date, result.DueDate?.Date);
		Assert.Equal(command.Priority, result.Priority);
	}

	[Fact]
	public async Task Handle_ShouldThrowFocusFlowNotFoundException_WhenTaskDoesNotExist()
	{
		// Arrange
		var command = new UpdateTaskCommand(
			Guid.NewGuid(),
			"Title",
			"Description",
			DateTime.UtcNow,
			Priority.Medium,
			null);

		_taskRepositoryMock.Setup(x => x.GetByIdAsync(command.TaskId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((ProjectTask)null!);

		// Act & Assert
		await Assert.ThrowsAsync<FocusFlowNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
	}
}
