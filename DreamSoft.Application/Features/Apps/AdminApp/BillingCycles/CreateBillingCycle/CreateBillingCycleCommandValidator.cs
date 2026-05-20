using FluentValidation;

namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.CreateBillingCycle;

public class CreateBillingCycleCommandValidator : AbstractValidator<CreateBillingCycleCommand>
{
    public CreateBillingCycleCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Months)
            .GreaterThan(0).WithMessage("Months must be greater than zero.");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Translations are required.");

        RuleFor(x => x.Translations.Spanish)
            .NotNull().WithMessage("Spanish translation is required.");

        RuleFor(x => x.Translations.Spanish.Name)
            .NotEmpty().WithMessage("Spanish name is required.")
            .MaximumLength(100).WithMessage("Spanish name must not exceed 100 characters.");

        RuleFor(x => x.Translations.English!.Name)
            .MaximumLength(100).WithMessage("English name must not exceed 100 characters.")
            .When(x => x.Translations?.English is not null);
    }
}
