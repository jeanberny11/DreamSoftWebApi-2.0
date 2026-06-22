using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Onboarding.GetOnboardingChecklist;

public record GetOnboardingChecklistQuery : IRequest<OnboardingChecklistResponse>;

public record OnboardingChecklistResponse(
    OnboardingVerificationsDto Verifications,
    OnboardingProfileSetupDto ProfileSetup,
    OnboardingSubscriptionDto Subscription
);

public record OnboardingVerificationsDto(
    bool EmailVerified
);

public record OnboardingProfileSetupDto(
    bool Completed
);

public record OnboardingSubscriptionDto(
    int ActiveSubscriptions
);
