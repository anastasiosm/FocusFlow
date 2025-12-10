using FluentValidation;
using FocusFlow.Application.Projects.Commands;

namespace FocusFlow.Application.Validators;

public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
	public DeleteProjectCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Project ID is required");
	}
}
