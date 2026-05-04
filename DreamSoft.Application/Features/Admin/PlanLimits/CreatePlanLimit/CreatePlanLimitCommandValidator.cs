using FluentValidation;

namespace DreamSoft.Application.Features.Admin.PlanLimits.CreatePlanLimit;

public class CreatePlanLimitCommandValidator : AbstractValidator<CreatePlanLimitCommand>
{
    public CreatePlanLimitCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("PlanId must be greater than zero.");

        RuleFor(x => x.LimitKey)
            .NotEmpty().WithMessage("LimitKey is required.")
            .MaximumLength(100).WithMessage("LimitKey must not exceed 100 characters.");

        RuleFor(x => x.LimitValue)
            .GreaterThanOrEqualTo(0).WithMessage("LimitValue cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
