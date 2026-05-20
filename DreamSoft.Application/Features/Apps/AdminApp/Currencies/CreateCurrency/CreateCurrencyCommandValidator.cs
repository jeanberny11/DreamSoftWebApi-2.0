using FluentValidation;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.CreateCurrency;

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(10).WithMessage("Code must not exceed 10 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.NativeName)
            .NotEmpty().WithMessage("Native name is required.")
            .MaximumLength(100).WithMessage("Native name must not exceed 100 characters.");
    }
}
