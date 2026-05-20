using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.GetPlanPrices;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record PlanPriceDto(
    int     Id,
    int     PlanId,
    int     BillingCycleId,
    string  BillingCycleCode,
    string  BillingCycleName,
    decimal Price,
    bool    IsActive);

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetPlanPricesQuery(int PlanId) : IRequest<IReadOnlyList<PlanPriceDto>>;

public class GetPlanPricesQueryHandler(IPlanPriceRepository planPriceRepository)
    : IRequestHandler<GetPlanPricesQuery, IReadOnlyList<PlanPriceDto>>
{
    public async Task<IReadOnlyList<PlanPriceDto>> Handle(
        GetPlanPricesQuery request, CancellationToken cancellationToken)
    {
        var prices = await planPriceRepository.GetByPlanIdAsync(request.PlanId, cancellationToken);

        return prices
            .Select(p => new PlanPriceDto(
                p.Id, p.PlanId,
                p.BillingCycleId,
                p.BillingCycle?.Code ?? string.Empty,
                p.BillingCycle?.Name ?? string.Empty,
                p.Price, p.IsActive))
            .ToList();
    }
}
