namespace DreamSoft.Application.Features.Registration.CheckOnboardingStatus;
using MediatR;

public record CheckOnboardingStatusQuery(int TenantId)
    : IRequest<OnboardingStatusResponse>;

public record OnboardingStatusResponse(
    int TenantId,
    bool OnboardingCompleted
);