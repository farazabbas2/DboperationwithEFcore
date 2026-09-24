using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Validators
{
    public class CreateNewColorValidator : AbstractValidator<CreateColorDto>
    {
        public CreateNewColorValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Color Name is required.")
                .MaximumLength(50).WithMessage("Color Name cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Color Name can only contain letters and spaces.");

        }
    }
}