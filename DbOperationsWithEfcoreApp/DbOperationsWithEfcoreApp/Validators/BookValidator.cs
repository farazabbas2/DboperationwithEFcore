using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Validators
{
    public class CreateBookValidator : AbstractValidator<CreateBookDto>
    {
        public CreateBookValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Book title is required.")
                .MaximumLength(50).WithMessage("Book title cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Book  title can only contain letters and spaces.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}