using FluentValidation;
using FocusFlow.BlazorApp.Features.Projects.Edit.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Validation;

public class ProjectUpdateFormModelValidator : AbstractValidator<ProjectUpdateFormModel>
{
    public ProjectUpdateFormModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .Length(3, 100).WithMessage("Project name must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
