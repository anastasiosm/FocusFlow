using FluentAssertions;
using FluentValidation.TestHelper;
using FocusFlow.Application.Features.Projects.CreateProject;
using FocusFlow.Application.Features.Projects.UpdateProject;
using FocusFlow.Application.Features.Projects.DeleteProject;

namespace FocusFlow.Application.Tests.Validators;

public class ProjectValidatorTests
{
	[Fact]
	public void CreateProjectCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new CreateProjectCommandValidator();
		var command = new CreateProjectCommand("Valid Project", "Description", "user123");

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void CreateProjectCommandValidator_WithEmptyName_ShouldHaveValidationError(string invalidName)
	{
		var validator = new CreateProjectCommandValidator();
		var command = new CreateProjectCommand(invalidName, "Description", "user123");

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Name)
			.WithErrorMessage("Project name is required");
	}

	[Fact]
	public void CreateProjectCommandValidator_WithNameTooLong_ShouldHaveValidationError()
	{
		var validator = new CreateProjectCommandValidator();
		var longName = new string('a', 201);
		var command = new CreateProjectCommand(longName, "Description", "user123");

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Name)
			.WithErrorMessage("Project name cannot exceed 200 characters");
	}

	[Fact]
	public void CreateProjectCommandValidator_WithDescriptionTooLong_ShouldHaveValidationError()
	{
		var validator = new CreateProjectCommandValidator();
		var longDescription = new string('a', 2001);
		var command = new CreateProjectCommand("Valid Name", longDescription, "user123");

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Description)
			.WithErrorMessage("Project description cannot exceed 2000 characters");
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	public void CreateProjectCommandValidator_WithEmptyOwnerId_ShouldHaveValidationError(string invalidOwnerId)
	{
		var validator = new CreateProjectCommandValidator();
		var command = new CreateProjectCommand("Valid Name", null, invalidOwnerId);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.OwnerId)
			.WithErrorMessage("Owner ID is required");
	}

	[Fact]
	public void UpdateProjectCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new UpdateProjectCommandValidator();
		var command = new UpdateProjectCommand(Guid.NewGuid(), "Valid Name", "Description", "user123");

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void UpdateProjectCommandValidator_WithEmptyId_ShouldHaveValidationError()
	{
		var validator = new UpdateProjectCommandValidator();
		var command = new UpdateProjectCommand(Guid.Empty, "Name", null, "user123");

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Id);
	}

	[Fact]
	public void DeleteProjectCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new DeleteProjectCommandValidator();
		var command = new DeleteProjectCommand(Guid.NewGuid(), "user123");

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void DeleteProjectCommandValidator_WithEmptyId_ShouldHaveValidationError()
	{
		var validator = new DeleteProjectCommandValidator();
		var command = new DeleteProjectCommand(Guid.Empty, "user123");

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Id)
			.WithErrorMessage("Project ID is required");
	}
}
