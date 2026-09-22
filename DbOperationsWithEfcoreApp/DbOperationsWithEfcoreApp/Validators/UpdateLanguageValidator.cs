using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Validators
{
    public class UpdateLanguageValidator : AbstractValidator<UpdateLanguageDto>
    {
        public UpdateLanguageValidator()
        {
           
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Language title is required.")
                .MaximumLength(50).WithMessage("Language title cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Language title can only contain letters and spaces.");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}