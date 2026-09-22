using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Validators
{
    public class CreateLanguageValidator : AbstractValidator<CreateLanguageDto>
    {
        public CreateLanguageValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Language title is required.")
                .MaximumLength(50).WithMessage("Language title cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Language title can only contain letters and spaces.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}