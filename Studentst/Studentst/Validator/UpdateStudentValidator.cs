using FluentValidation;
using Studentst.DTOs;

namespace Studentst.Validators
{
    public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentValidator()
        {
            // Same rules as Create, but Age optional ho sakta hai
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

         /*   RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");*/

            RuleFor(x => x.age)
                .InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100")
                .When(x => x.age > 0); // Optional for update

            RuleFor(x => x.PhoneNo)
                .Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits")
                .When(x => !string.IsNullOrEmpty(x.PhoneNo));
        }
    }
}