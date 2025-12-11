using FluentValidation;

namespace FocusFlow.Application.Features.Tasks.DeleteTask;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
	public DeleteTaskCommandValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");
	}
}
