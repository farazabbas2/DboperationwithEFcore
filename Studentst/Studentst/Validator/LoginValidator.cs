using FluentValidation;
using Studentst.DTOs;

namespace Studentst.Validators
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            // Email Validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            // Password Validation
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}