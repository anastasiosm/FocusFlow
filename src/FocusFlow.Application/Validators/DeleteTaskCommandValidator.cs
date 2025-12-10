using FluentValidation;
using FocusFlow.Application.Tasks.Commands;

namespace FocusFlow.Application.Validators;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
	public DeleteTaskCommandValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");
	}
}
