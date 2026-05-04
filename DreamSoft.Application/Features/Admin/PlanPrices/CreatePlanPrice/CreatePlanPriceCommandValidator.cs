using FluentValidation;

namespace DreamSoft.Application.Features.Admin.PlanPrices.CreatePlanPrice;

public class CreatePlanPriceCommandValidator : AbstractValidator<CreatePlanPriceCommand>
{
    public CreatePlanPriceCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("PlanId must be greater than zero.");

        RuleFor(x => x.BillingCycleId)
            .GreaterThan(0).WithMessage("BillingCycleId must be greater than zero.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
    }
}
