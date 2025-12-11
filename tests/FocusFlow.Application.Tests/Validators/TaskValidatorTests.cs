using FluentAssertions;
using FluentValidation.TestHelper;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Application.Features.Tasks.UpdateTask;
using FocusFlow.Application.Features.Tasks.DeleteTask;
using FocusFlow.Application.Features.Tasks.AssignTask;
using FocusFlow.Application.Features.Tasks.UpdateTaskStatus;
using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.Tests.Validators;

public class TaskValidatorTests
{
	[Fact]
	public void CreateTaskCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new CreateTaskCommandValidator();
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			"Valid Task",
			"Description",
			DateTime.UtcNow.AddDays(7),
			Priority.Medium,
			null);

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void CreateTaskCommandValidator_WithEmptyTitle_ShouldHaveValidationError(string invalidTitle)
	{
		var validator = new CreateTaskCommandValidator();
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			invalidTitle,
			null,
			null,
			Priority.Medium,
			null);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Title)
			.WithErrorMessage("Task title is required");
	}

	[Fact]
	public void CreateTaskCommandValidator_WithTitleTooLong_ShouldHaveValidationError()
	{
		var validator = new CreateTaskCommandValidator();
		var longTitle = new string('a', 201);
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			longTitle,
			null,
			null,
			Priority.Medium,
			null);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Title)
			.WithErrorMessage("Task title cannot exceed 200 characters");
	}

	[Fact]
	public void CreateTaskCommandValidator_WithPastDueDate_ShouldHaveValidationError()
	{
		var validator = new CreateTaskCommandValidator();
		var pastDate = DateTime.UtcNow.AddDays(-2);
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			"Task",
			null,
			pastDate,
			Priority.Medium,
			null);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.DueDate)
			.WithErrorMessage("Due date cannot be in the past");
	}

	[Fact]
	public void CreateTaskCommandValidator_WithNullDueDate_ShouldNotHaveValidationError()
	{
		var validator = new CreateTaskCommandValidator();
		var command = new CreateTaskCommand(
			Guid.NewGuid(),
			"Task",
			null,
			null,
			Priority.Medium,
			null);

		var result = validator.TestValidate(command);

		result.ShouldNotHaveValidationErrorFor(c => c.DueDate);
	}

	[Fact]
	public void UpdateTaskCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new UpdateTaskCommandValidator();
		var command = new UpdateTaskCommand(
			Guid.NewGuid(),
			"Valid Title",
			"Valid Description",
			DateTime.UtcNow.AddDays(1),
			Priority.High);

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void UpdateTaskCommandValidator_WithEmptyId_ShouldHaveValidationError()
	{
		var validator = new UpdateTaskCommandValidator();
		var command = new UpdateTaskCommand(
			Guid.Empty,
			"Valid Title",
			null,
			null,
			Priority.Low);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.TaskId);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void UpdateTaskCommandValidator_WithEmptyTitle_ShouldHaveValidationError(string invalidTitle)
	{
		var validator = new UpdateTaskCommandValidator();
		var command = new UpdateTaskCommand(
			Guid.NewGuid(),
			invalidTitle,
			null,
			null,
			Priority.Low);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Title)
			.WithErrorMessage("Task title is required");
	}

	[Fact]
	public void UpdateTaskCommandValidator_WithTitleTooLong_ShouldHaveValidationError()
	{
		var validator = new UpdateTaskCommandValidator();
		var longTitle = new string('a', 201);
		var command = new UpdateTaskCommand(
			Guid.NewGuid(),
			longTitle,
			null,
			null,
			Priority.Low);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.Title)
			.WithErrorMessage("Task title cannot exceed 200 characters");
	}

	[Fact]
	public void UpdateTaskCommandValidator_WithPastDueDate_ShouldHaveValidationError()
	{
		var validator = new UpdateTaskCommandValidator();
		var pastDate = DateTime.UtcNow.AddDays(-2);
		var command = new UpdateTaskCommand(
			Guid.NewGuid(),
			"Task",
			null,
			pastDate,
			Priority.Medium);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.DueDate)
			.WithErrorMessage("Due date cannot be in the past");
	}

	[Fact]
	public void DeleteTaskCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new DeleteTaskCommandValidator();
		var command = new DeleteTaskCommand(Guid.NewGuid());

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void DeleteTaskCommandValidator_WithEmptyId_ShouldHaveValidationError()
	{
		var validator = new DeleteTaskCommandValidator();
		var command = new DeleteTaskCommand(Guid.Empty);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.TaskId)
			.WithErrorMessage("Task ID is required");
	}

	[Fact]
	public void AssignTaskCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new AssignTaskCommandValidator();
		var command = new AssignTaskCommand(Guid.NewGuid(), "user123");

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void AssignTaskCommandValidator_WithEmptyTaskId_ShouldHaveValidationError()
	{
		var validator = new AssignTaskCommandValidator();
		var command = new AssignTaskCommand(Guid.Empty, "user123");

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.TaskId)
			.WithErrorMessage("Task ID is required");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void AssignTaskCommandValidator_WithEmptyUserId_ShouldHaveValidationError(string invalidUserId)
	{
		var validator = new AssignTaskCommandValidator();
		var command = new AssignTaskCommand(Guid.NewGuid(), invalidUserId);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.UserId)
			.WithErrorMessage("User ID is required");
	}

	[Fact]
	public void UpdateTaskStatusCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
	{
		var validator = new UpdateTaskStatusCommandValidator();
		var command = new UpdateTaskStatusCommand(Guid.NewGuid(), ProjectTaskStatus.InProgress);

		var result = validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void UpdateTaskStatusCommandValidator_WithEmptyTaskId_ShouldHaveValidationError()
	{
		var validator = new UpdateTaskStatusCommandValidator();
		var command = new UpdateTaskStatusCommand(Guid.Empty, ProjectTaskStatus.InProgress);

		var result = validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(c => c.TaskId)
			.WithErrorMessage("Task ID is required");
	}
}
