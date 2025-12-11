using FluentValidation;

namespace FocusFlow.Application.Features.Tasks.AssignTask;

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
