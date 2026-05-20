namespace DreamSoft.Application.Features.Apps.LandingApp.Registration.CheckOnboardingStatus;
using MediatR;

public record CheckOnboardingStatusQuery(int TenantId)
    : IRequest<OnboardingStatusResponse>;

public record OnboardingStatusResponse(
    int TenantId,
    bool OnboardingCompleted
);