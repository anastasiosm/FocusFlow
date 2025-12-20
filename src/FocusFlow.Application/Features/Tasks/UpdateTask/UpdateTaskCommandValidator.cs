using FluentValidation;

namespace FocusFlow.Application.Features.Tasks.UpdateTask;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
	public UpdateTaskCommandValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Task title is required")
			.MaximumLength(200).WithMessage("Task title cannot exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(2000).WithMessage("Task description cannot exceed 2000 characters")
			.When(x => x.Description != null);

		RuleFor(x => x.DueDate)
			.GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Due date cannot be in the past")
			.When(x => x.DueDate.HasValue);

		RuleFor(x => x.Priority)
			.IsInEnum().WithMessage("Invalid priority level");
	}
}
