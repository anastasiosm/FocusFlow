using FluentValidation;
using FocusFlow.Application.Tasks.Commands;

namespace FocusFlow.Application.Validators;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.DueDate)
            .Must(BeAValidDate).When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be a valid date.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority level.");
    }

    private bool BeAValidDate(DateTime? date)
    {
        return date.HasValue && date.Value.Date >= DateTime.Today.Date;
    }
}
