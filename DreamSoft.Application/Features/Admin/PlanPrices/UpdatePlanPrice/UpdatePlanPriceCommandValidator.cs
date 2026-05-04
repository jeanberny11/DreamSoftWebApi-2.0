using FluentValidation;

namespace DreamSoft.Application.Features.Admin.PlanPrices.UpdatePlanPrice;

public class UpdatePlanPriceCommandValidator : AbstractValidator<UpdatePlanPriceCommand>
{
    public UpdatePlanPriceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than zero.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
    }
}
