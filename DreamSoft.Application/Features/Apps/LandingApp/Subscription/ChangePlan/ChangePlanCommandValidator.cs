using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.ChangePlan;

public class ChangePlanCommandValidator : AbstractValidator<ChangePlanCommand>
{
    public ChangePlanCommandValidator()
    {
        RuleFor(x => x.NewPlanPriceId)
            .PositiveId();
    }
}
