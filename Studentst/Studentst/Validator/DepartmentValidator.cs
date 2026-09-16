using FluentValidation;
using Studentst.DTOs;

namespace Studentst.Validators
{
    public class DepartmentValidator : AbstractValidator<CreateDepartmentDto>
    {
        public DepartmentValidator()
        {
            // Name Validation
            RuleFor(x => x.name)
                .NotEmpty().WithMessage("Department name is required")
                .MaximumLength(100).WithMessage("Department name cannot exceed 20 characters")
                .Matches(@"^[a-zA-Z0-9\s\-&]+$").WithMessage("Department name can only contain letters, numbers, spaces, hyphens and ampersands");
            // Description Validation (Optional)
            RuleFor(x => x.description)
                 .NotEmpty().WithMessage("Description name is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
                

        }
    }

    // Update Validator
    
}