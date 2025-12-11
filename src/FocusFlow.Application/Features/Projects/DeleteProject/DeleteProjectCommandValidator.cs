using FluentValidation;

namespace FocusFlow.Application.Features.Projects.DeleteProject;

public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
	public DeleteProjectCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Project ID is required");
	}
}
