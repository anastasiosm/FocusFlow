using FluentAssertions;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Tasks.Commands;

public class CreateTaskCommandTests : TestBase
{
	private readonly CreateTaskCommandHandler _handler;

	public CreateTaskCommandTests()
	{
		_handler = new CreateTaskCommandHandler(
			MockTaskRepository.Object,
			MockProjectRepository.Object,
			MockUnitOfWork.Object,
			Mapper,
			MockEventPublisher.Object);
	}

	[Fact]
	public async Task Handle_WithValidCommand_ShouldCreateTask()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var project = new Project("Test Project", null, "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(project);

		var dueDate = DateTime.UtcNow.AddDays(7);
		var command = new CreateTaskCommand(
			projectId,
			"Test Task",
			"Task Description",
			dueDate,
			Priority.High,
			"user123");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Test Task");
		result.Description.Should().Be("Task Description");
		result.Priority.Should().Be(Priority.High);
		result.Status.Should().Be(ProjectTaskStatus.Todo);
		result.ProjectId.Should().Be(projectId);
		result.AssignedUserId.Should().Be("user123");
		result.DueDate.Should().BeCloseTo(dueDate, TimeSpan.FromSeconds(1));

		MockTaskRepository.Verify(
			repo => repo.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()),
			Times.Once);

		VerifyUnitOfWorkSaveChanges(Times.Once());
	}

	[Fact]
	public async Task Handle_WithNonExistingProject_ShouldThrowNotFoundException()
	{
		// Arrange
		var projectId = Guid.NewGuid();

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Project?)null);

		var command = new CreateTaskCommand(
			projectId,
			"Task",
			null,
			null,
			Priority.Medium,
			null);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowNotFoundException>()
			.WithMessage("*Project*not found*");

		MockTaskRepository.Verify(
			repo => repo.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()),
			Times.Never);

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Fact]
	public async Task Handle_WithDefaultValues_ShouldCreateTaskWithDefaults()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var project = new Project("Project", null, "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(project);

		var command = new CreateTaskCommand(
			projectId,
			"Simple Task",
			null,
			null,
			Priority.Medium,
			null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Status.Should().Be(ProjectTaskStatus.Todo);
		result.Priority.Should().Be(Priority.Medium);
		result.DueDate.Should().BeNull();
		result.AssignedUserId.Should().BeNull();
		result.Description.Should().BeNull();

		MockTaskRepository.Verify(
			repo => repo.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()),
			Times.Once);
		VerifyUnitOfWorkSaveChanges(Times.Once());
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task Handle_WithInvalidTitle_ShouldThrowValidationException(string invalidTitle)
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var project = new Project("Project", null, "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(project, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(project);

		var command = new CreateTaskCommand(
			projectId,
			invalidTitle,
			"Description",
			null,
			Priority.Medium,
			null);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowValidationException>()
			.WithMessage("*title cannot be empty*");

		MockTaskRepository.Verify(
			repo => repo.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()),
			Times.Never);
		VerifyUnitOfWorkSaveChanges(Times.Never());
	}
}