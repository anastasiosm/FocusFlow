using FluentAssertions;
using FluentValidation.TestHelper;
using FocusFlow.Application.Projects.Commands;
using FocusFlow.Application.Tasks.Commands;
using FocusFlow.Application.Validators;
using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.Tests.Validators;

public class ValidatorTests
{
	[Fact]
	public void CreateProjectCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		// Arrange
		var validator = new CreateProjectCommandValidator();
		var command = new CreateProjectCommand("Valid Project", "Description", "user123");

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void CreateProjectCommandValidator_WithEmptyName_ShouldHaveValidationError(string invalidName)
	{
		// Arrange
		var validator = new CreateProjectCommandValidator();
		var command = new CreateProjectCommand(invalidName, "Description", "user123");

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Name)
			.WithErrorMessage("Project name is required");
	}

	[Fact]
	public void CreateProjectCommandValidator_WithNameTooLong_ShouldHaveValidationError()
	{
		// Arrange
		var validator = new CreateProjectCommandValidator();
		var longName = new string('a', 201);
		var command = new CreateProjectCommand(longName, "Description", "user123");

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Name)
			.WithErrorMessage("Project name cannot exceed 200 characters");
	}

	[Fact]
	public void CreateProjectCommandValidator_WithDescriptionTooLong_ShouldHaveValidationError()
	{
		// Arrange
		var validator = new CreateProjectCommandValidator();
		var longDescription = new string('a', 2001);
		var command = new CreateProjectCommand("Valid Name", longDescription, "user123");

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Description)
			.WithErrorMessage("Project description cannot exceed 2000 characters");
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	public void CreateProjectCommandValidator_WithEmptyOwnerId_ShouldHaveValidationError(string invalidOwnerId)
	{
		// Arrange
		var validator = new CreateProjectCommandValidator();
		var command = new CreateProjectCommand("Valid Name", null, invalidOwnerId);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.OwnerId)
			.WithErrorMessage("Owner ID is required");
	}

	[Fact]
	public void UpdateProjectCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		// Arrange
		var validator = new UpdateProjectCommandValidator();
		var command = new UpdateProjectCommand(Guid.NewGuid(), "Valid Name", "Description");

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void UpdateProjectCommandValidator_WithEmptyId_ShouldHaveValidationError()
	{
		// Arrange
		var validator = new UpdateProjectCommandValidator();
		var command = new UpdateProjectCommand(Guid.Empty, "Name", null);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Id);
	}

	[Fact]
	public void CreateTaskCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		// Arrange
		var validator = new CreateTaskCommandValidator();
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			"Valid Task",
			"Description",
			DateTime.UtcNow.AddDays(7),
			Priority.Medium,
			null);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void CreateTaskCommandValidator_WithEmptyTitle_ShouldHaveValidationError(string invalidTitle)
	{
		// Arrange
		var validator = new CreateTaskCommandValidator();
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			invalidTitle,
			null,
			null,
			Priority.Medium,
			null);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Title)
			.WithErrorMessage("Task title is required");
	}

	[Fact]
	public void CreateTaskCommandValidator_WithTitleTooLong_ShouldHaveValidationError()
	{
		// Arrange
		var validator = new CreateTaskCommandValidator();
		var longTitle = new string('a', 201);
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			longTitle,
			null,
			null,
			Priority.Medium,
			null);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Title)
			.WithErrorMessage("Task title cannot exceed 200 characters");
	}

	[Fact]
	public void CreateTaskCommandValidator_WithPastDueDate_ShouldHaveValidationError()
	{
		// Arrange
		var validator = new CreateTaskCommandValidator();
		var pastDate = DateTime.UtcNow.AddDays(-2);
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			"Task",
			null,
			pastDate,
			Priority.Medium,
			null);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.DueDate)
			.WithErrorMessage("Due date cannot be in the past");
	}

	[Fact]
	public void CreateTaskCommandValidator_WithNullDueDate_ShouldNotHaveValidationError()
	{
		// Arrange
		var validator = new CreateTaskCommandValidator();
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			"Task",
			null,
			null,
			Priority.Medium,
			null);

		// Act
		var result = validator.TestValidate(command);

		// Assert
		result.ShouldNotHaveValidationErrorFor(c => c.DueDate);
	}
}