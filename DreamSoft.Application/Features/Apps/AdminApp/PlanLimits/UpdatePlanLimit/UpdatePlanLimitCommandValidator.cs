using FluentValidation;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.UpdatePlanLimit;

public class UpdatePlanLimitCommandValidator : AbstractValidator<UpdatePlanLimitCommand>
{
    public UpdatePlanLimitCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than zero.");

        RuleFor(x => x.LimitValue)
            .GreaterThanOrEqualTo(0).WithMessage("LimitValue cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
