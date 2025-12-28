using FluentValidation;
using FocusFlow.BlazorApp.Models.Tasks;

namespace FocusFlow.BlazorApp.Validation.Tasks;

public class TaskFormModelValidator : AbstractValidator<TaskFormModel>
{
	public TaskFormModelValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title must not exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

		RuleFor(x => x.DueDate)
			// FIX: Use UTC to match backend validation
			.GreaterThanOrEqualTo(DateTime.UtcNow.Date)
			.When(x => x.DueDate.HasValue)
			.WithMessage("Due date cannot be in the past");

		RuleFor(x => x.Priority)
			.IsInEnum().WithMessage("Invalid priority");
	}

	/// <summary>
	/// Field-level validation for MudBlazor components.
	/// Validates a single property when it changes.
	/// </summary>
	public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
	{
		var result = await ValidateAsync(
			ValidationContext<TaskFormModel>.CreateWithOptions(
				(TaskFormModel)model,
				x => x.IncludeProperties(propertyName)
			)
		);

		if (result.IsValid)
			return Array.Empty<string>();

		return result.Errors.Select(e => e.ErrorMessage);
	};
}