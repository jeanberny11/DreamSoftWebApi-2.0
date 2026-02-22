using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Onboarding.CompleteOnboarding;

public class CompleteOnboardingCommandValidator
    : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.SolutionId).PositiveId();
        RuleFor(x => x.SubscriptionPlanId).PositiveId();
    }
}
