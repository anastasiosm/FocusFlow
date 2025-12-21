using FluentValidation;
using FocusFlow.BlazorApp.Models;

namespace FocusFlow.BlazorApp.Validators;

public class TaskEditModelValidator : AbstractValidator<TaskEditModel>
{
	public TaskEditModelValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title must not exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

		RuleFor(x => x.DueDate)
			.GreaterThanOrEqualTo(DateTime.Today).WithMessage("Due date must be today or in the future")
			.When(x => x.DueDate.HasValue);
	}
}
