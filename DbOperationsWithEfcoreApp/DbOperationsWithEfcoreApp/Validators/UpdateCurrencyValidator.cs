using FluentValidation;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Validators
{
    public class UpdateCurrencyValidator : AbstractValidator<UpdateCurrencyDto>
    {
        public UpdateCurrencyValidator()
        {
           

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Currency title is required.")
                .MaximumLength(50).WithMessage("Currency title cannot exceed 50 characters.");

            RuleFor(x => x.description)
                .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");
        }
    }
}