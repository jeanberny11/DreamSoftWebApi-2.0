using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.GetPlanLimits;

public record GetPlanLimitsQuery(int PlanId) : IRequest<IReadOnlyList<PlanLimitDto>>;

public class GetPlanLimitsQueryHandler(IPlanLimitRepository planLimitRepository)
    : IRequestHandler<GetPlanLimitsQuery, IReadOnlyList<PlanLimitDto>>
{
    public async Task<IReadOnlyList<PlanLimitDto>> Handle(
        GetPlanLimitsQuery request, CancellationToken cancellationToken)
    {
        var limits = await planLimitRepository.GetByPlanIdAsync(request.PlanId, cancellationToken);

        return limits
            .Select(l => new PlanLimitDto(l.Id, l.PlanId, l.LimitKey, l.LimitValue, l.Description))
            .ToList();
    }
}
