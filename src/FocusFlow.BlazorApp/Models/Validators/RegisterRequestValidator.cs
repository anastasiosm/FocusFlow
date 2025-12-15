using FluentValidation;

namespace FocusFlow.BlazorApp.Models.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("A valid email is required.");
		RuleFor(x => x.Username)
	.NotEmpty().WithMessage("Username is required.")
	.Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
			.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.");
	}
}
