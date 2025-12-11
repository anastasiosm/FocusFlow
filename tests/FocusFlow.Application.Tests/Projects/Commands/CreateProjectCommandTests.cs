using FluentAssertions;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Domain.Entities;
using Moq;

namespace FocusFlow.Application.Tests.Projects.Commands;

public class CreateProjectCommandTests : TestBase
{
	private readonly CreateProjectCommandHandler _handler;

	public CreateProjectCommandTests()
	{
		_handler = new CreateProjectCommandHandler(
			MockProjectRepository.Object,
			MockUnitOfWork.Object,
			Mapper);
	}

	[Fact]
	public async Task Handle_WithValidCommand_ShouldCreateProject()
	{
		// Arrange
		var command = new CreateProjectCommand(
			"Test Project",
			"Test Description",
			"user123");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Name.Should().Be("Test Project");
		result.Description.Should().Be("Test Description");
		result.OwnerId.Should().Be("user123");
		result.Id.Should().NotBeEmpty();

		MockProjectRepository.Verify(
			repo => repo.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
			Times.Once);

		VerifyUnitOfWorkSaveChanges(Times.Once());
	}

	[Fact]
	public async Task Handle_WithValidCommand_ShouldSetCreatedAndUpdatedDates()
	{
		// Arrange
		var command = new CreateProjectCommand("Project", null, "user123");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
		result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Handle_WithNullDescription_ShouldCreateProjectWithNullDescription()
	{
		// Arrange
		var command = new CreateProjectCommand("Project", null, "user123");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Description.Should().BeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task Handle_WithInvalidName_ShouldThrowValidationException(string invalidName)
	{
		// Arrange
		var command = new CreateProjectCommand(invalidName, "Description", "user123");

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>(); // Domain validation exception

		MockProjectRepository.Verify(
			repo => repo.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
			Times.Never);

		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Fact]
	public async Task Handle_WithLongName_ShouldThrowValidationException()
	{
		// Arrange
		var longName = new string('a', 201);
		var command = new CreateProjectCommand(longName, "Description", "user123");

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<Exception>();
		VerifyUnitOfWorkSaveChanges(Times.Never());
	}

	[Fact]
	public async Task Handle_ShouldMapProjectToDto()
	{
		// Arrange
		var command = new CreateProjectCommand("Project", "Description", "user123");

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.TaskCount.Should().Be(0); // New project has no tasks
	}
}