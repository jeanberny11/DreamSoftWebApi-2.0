using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Subscription.CancelSubscription;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.SolutionId)
            .PositiveId();

        RuleFor(x => x.CancellationReason)
            .MaximumLength(100)
            .When(x => x.CancellationReason is not null);

        RuleFor(x => x.CancellationFeedback)
            .MaximumLength(1000)
            .When(x => x.CancellationFeedback is not null);
    }
}
