using FluentValidation;
using Studentst.DTOs;  // Apna namespace check karo

namespace Studentst.Validators
{
    public class StudentValidator : AbstractValidator<StudentDto>
    {
        public StudentValidator()
        {
            // Name Validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces");

            // Email Validation
          /*  RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Email must be a valid email address");*/

            // Age Validation
            RuleFor(x => x.Age)
                .InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100")
                .NotEmpty().WithMessage("Age is required");

            // Phone Number Validation (10 digits)
            RuleFor(x => x.PhoneNo)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits")
                .When(x => !string.IsNullOrEmpty(x.PhoneNo));

            // Department Validation (Optional)
           /* RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Department ID must be greater than 0")
                .When(x => x.DepartmentId.HasValue);*/
        }
    }
}