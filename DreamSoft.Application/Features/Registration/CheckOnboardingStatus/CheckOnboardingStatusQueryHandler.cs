namespace DreamSoft.Application.Features.Registration.CheckOnboardingStatus;

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;
public class CheckOnboardingStatusQueryHandler(ITenantRepository repository) : IRequestHandler<CheckOnboardingStatusQuery, OnboardingStatusResponse>
{
    public Task<OnboardingStatusResponse> Handle(CheckOnboardingStatusQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;
        var tenant = repository.GetByIdAsync(tenantId, cancellationToken).Result ?? throw new NotFoundException("TenantNotFound", tenantId);

        var response = new OnboardingStatusResponse(
            TenantId: tenantId,
            OnboardingCompleted: tenant.OnboardingCompleted
        );
        return Task.FromResult(response);
    }
}