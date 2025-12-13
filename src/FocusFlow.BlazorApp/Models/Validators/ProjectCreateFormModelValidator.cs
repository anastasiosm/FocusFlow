using FluentValidation;
using FocusFlow.BlazorApp.Models; // For ProjectCreateFormModel

namespace FocusFlow.BlazorApp.Models.Validators;

public class ProjectCreateFormModelValidator : AbstractValidator<ProjectCreateFormModel>
{
    public ProjectCreateFormModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .Length(3, 100).WithMessage("Project name must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
