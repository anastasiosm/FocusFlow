using FluentAssertions;
using FluentValidation.TestHelper;
using FocusFlow.Application.Features.Tasks.AssignTask;
using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.Tests.Features.Tasks.AssignTask;

public class AssignTaskCommandValidatorTests
{
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
}
