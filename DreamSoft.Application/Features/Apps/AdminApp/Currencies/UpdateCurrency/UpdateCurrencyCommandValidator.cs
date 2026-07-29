using FluentValidation;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.UpdateCurrency;

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.NativeName)
            .NotEmpty().WithMessage("Native name is required.")
            .MaximumLength(100).WithMessage("Native name must not exceed 100 characters.");
    }
}
