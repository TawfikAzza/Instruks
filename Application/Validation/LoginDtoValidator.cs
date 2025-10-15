using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validation
{
    /// <summary>
    /// Validation rules for <see cref="LoginDto"/>.
    /// <para>
    /// Keep these rules minimal: login should accept whatever the user registered with.
    /// Enforce password complexity at registration (not here) to avoid locking out legit users.
    /// </para>
    /// </summary>
    public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDtoValidator"/> class.
        /// </summary>
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256).WithMessage("Email must be at most 256 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MaximumLength(128).WithMessage("Password must be at most 128 characters.");
            // NOTE: No complexity checks here by design (see class summary).
        }
    }
}