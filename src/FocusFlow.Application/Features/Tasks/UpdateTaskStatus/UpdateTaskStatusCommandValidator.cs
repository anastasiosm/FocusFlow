using FluentValidation;

namespace FocusFlow.Application.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
	public UpdateTaskStatusCommandValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");

		RuleFor(x => x.Status)
			.IsInEnum().WithMessage("Invalid task status");
	}
}
