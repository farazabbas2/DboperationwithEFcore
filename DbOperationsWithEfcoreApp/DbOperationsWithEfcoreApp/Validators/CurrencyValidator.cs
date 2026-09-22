using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Validators
{
    public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyDto>
    {
        public CreateCurrencyValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Currency title is required.")
                .MaximumLength(50).WithMessage("Currency title cannot exceed 50 characters.");

            RuleFor(x => x.description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");
        }
    }
}