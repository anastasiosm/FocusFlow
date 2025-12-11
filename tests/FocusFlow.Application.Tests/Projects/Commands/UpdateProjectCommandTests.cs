using FluentAssertions;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;
using Moq;

namespace FocusFlow.Application.Tests.Projects.Commands;

public class UpdateProjectCommandTests : TestBase
{
	private readonly UpdateProjectCommandHandler _handler;

	public UpdateProjectCommandTests()
	{
		_handler = new UpdateProjectCommandHandler(
			MockProjectRepository.Object,
			MockUnitOfWork.Object);
	}

	[Fact]
	public async Task Handle_WithExistingProject_ShouldUpdateProject()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var existingProject = new Project("Original Name", "Original Description", "user123");

		// Use reflection to set Id (since it's private set)
		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(existingProject, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProject);

		var command = new UpdateProjectCommand(projectId, "Updated Name", "Updated Description");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		existingProject.Name.Should().Be("Updated Name");
		existingProject.Description.Should().Be("Updated Description");

		MockProjectRepository.Verify(
			repo => repo.UpdateAsync(existingProject, It.IsAny<CancellationToken>()),
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

		var command = new UpdateProjectCommand(projectId, "Updated Name", null);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowNotFoundException>()
			.WithMessage("*Project*not found*");

		MockProjectRepository.Verify(
			repo => repo.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
			Times.Never);

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task Handle_WithInvalidName_ShouldThrowValidationException(string invalidName)
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var existingProject = new Project("Original", "Description", "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(existingProject, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProject);

		var command = new UpdateProjectCommand(projectId, invalidName, "Description");

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowValidationException>()
			.WithMessage("*name cannot be empty*");
		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Fact]
	public async Task Handle_WithDescriptionTooLong_ShouldThrowValidationException()
	{
		// Arrange
		var projectId = Guid.NewGuid();
		var existingProject = new Project("Original", "Description", "user123");

		var idProperty = typeof(Project).GetProperty("Id");
		idProperty!.SetValue(existingProject, projectId);

		MockProjectRepository
			.Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProject);

		var longDescription = new string('a', 2001);
		var command = new UpdateProjectCommand(projectId, "Valid Name", longDescription);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<FocusFlowValidationException>()
			.WithMessage("*cannot exceed 2000 characters*");
	}
}