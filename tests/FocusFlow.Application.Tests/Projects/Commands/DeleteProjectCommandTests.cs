using FluentAssertions;
using FocusFlow.Application.Features.Projects.DeleteProject;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Projects.Commands;

public class DeleteProjectCommandTests : TestBase
{
	private readonly DeleteProjectCommandHandler _handler;

	public DeleteProjectCommandTests()
	{
		_handler = new DeleteProjectCommandHandler(
			MockProjectRepository.Object,
			MockUnitOfWork.Object);
	}

	[Fact]
	public async Task Handle_WithExistingProject_ShouldDeleteProject()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var existingProject = new Project("Project to Delete", null, "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(existingProject, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProject);

		var command = new DeleteProjectCommand(projectId);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		MockProjectRepository.Verify(
			repo => repo.DeleteAsync(existingProject, It.IsAny<CancellationToken>()),
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

		var command = new DeleteProjectCommand(projectId);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowNotFoundException>()
			.WithMessage("*Project*not found*");

		MockProjectRepository.Verify(
			repo => repo.DeleteAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
			Times.Never);

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Fact]
	public async Task Handle_ShouldCallRepositoryWithCorrectProject()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var existingProject = new Project("Project", null, "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(existingProject, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProject);

		var command = new DeleteProjectCommand(projectId);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		MockProjectRepository.Verify(
			repo => repo.DeleteAsync(
				It.Is<Project>(p => p.Id == projectId && p.Name == "Project"),
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		
		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		var command = new DeleteProjectCommand(projectId);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Fact]
	public async Task Handle_WhenDeleteFails_ShouldNotSaveChanges()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var existingProject = new Project("Project", null, "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(existingProject, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProject);
		
		MockProjectRepository
			.Setup(repo => repo.DeleteAsync(existingProject, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Delete failed"));

		var command = new DeleteProjectCommand(projectId);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
		VerifyUnitOfWorkSaveChanges(Times.Never());
	}
}   