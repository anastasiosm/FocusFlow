using FluentValidation;
using FocusFlow.Application.Tasks.Commands;

namespace FocusFlow.Application.Validators;

public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
	public AssignTaskCommandValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");

		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("User ID is required");
	}
}
