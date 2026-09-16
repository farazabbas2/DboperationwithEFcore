using FluentValidation;
using Studentst.DTOs; // Apne project ke DTO namespace ke hisaab se adjust karein

namespace Studentst.Validators
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
    {
        public UpdateDepartmentValidator()
        {
            // 1. Name Validation (Mandatory)
            RuleFor(x => x.name)
                .NotEmpty().WithMessage("Department name is required")
                .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-&]+$").WithMessage("Department name can only contain letters, numbers, spaces, hyphens and ampersands");

            // 2. Description Validation (Optional)
            RuleFor(x => x.description)
                 .NotEmpty().WithMessage("Description  field is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
             
        }
    }
}