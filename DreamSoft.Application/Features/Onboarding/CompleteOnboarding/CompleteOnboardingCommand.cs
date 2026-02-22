using MediatR;

namespace DreamSoft.Application.Features.Onboarding.CompleteOnboarding;

public record CompleteOnboardingCommand(
    int SolutionId,
    int SubscriptionPlanId
) : IRequest<CompleteOnboardingResponse>;

public record CompleteOnboardingResponse(string RedirectUrl);
