using FluentValidation;

namespace DreamSoft.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than zero.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.TierLevel)
            .GreaterThan(0).WithMessage("TierLevel must be greater than zero.");

        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0).WithMessage("TrialDays cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);

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
