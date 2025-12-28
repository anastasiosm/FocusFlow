using FluentValidation;
using FocusFlow.BlazorApp.Features.Tasks.Create.Models;

namespace FocusFlow.BlazorApp.Features.Tasks.Create.Validation;

/// <summary>
/// Frontend validation for CreateTaskFormModel.
/// Note: This does basic validation only. Backend does authoritative validation with UTC.
/// </summary>
public class CreateTaskFormModelValidator : AbstractValidator<CreateTaskFormModel>
{
	public CreateTaskFormModelValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title must not exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

		// LIGHT validation - just check it's not null if set
		// Let backend do the authoritative date validation with UTC
		RuleFor(x => x.DueDate)
			.Must(date => !date.HasValue || date.Value.Date >= DateTime.Today)
			.When(x => x.DueDate.HasValue)
			.WithMessage("Due date cannot be in the past");

		RuleFor(x => x.Priority)
			.IsInEnum().WithMessage("Invalid priority");
	}
}