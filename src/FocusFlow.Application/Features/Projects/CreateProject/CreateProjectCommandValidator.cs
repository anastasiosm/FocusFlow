using FluentValidation;

namespace FocusFlow.Application.Features.Projects.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
	public CreateProjectCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Project name is required")
			.MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(2000).WithMessage("Project description cannot exceed 2000 characters")
			.When(x => x.Description != null);

		RuleFor(x => x.OwnerId)
			.NotEmpty().WithMessage("Owner ID is required");
	}
}
