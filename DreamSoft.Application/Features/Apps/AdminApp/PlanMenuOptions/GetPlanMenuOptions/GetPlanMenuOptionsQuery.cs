using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.GetPlanMenuOptions;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record PlanMenuOptionDto(
    int    PlanId,
    int    MenuOptionId,
    string MenuOptionCode,
    string MenuOptionName);

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetPlanMenuOptionsQuery(int PlanId) : IRequest<IReadOnlyList<PlanMenuOptionDto>>;

public class GetPlanMenuOptionsQueryHandler(IPlanMenuOptionRepository planMenuOptionRepository)
    : IRequestHandler<GetPlanMenuOptionsQuery, IReadOnlyList<PlanMenuOptionDto>>
{
    public async Task<IReadOnlyList<PlanMenuOptionDto>> Handle(
        GetPlanMenuOptionsQuery request, CancellationToken cancellationToken)
    {
        var entries = await planMenuOptionRepository.GetByPlanIdAsync(request.PlanId, cancellationToken);

        return entries
            .Select(e => new PlanMenuOptionDto(
                e.PlanId, e.MenuOptionId,
                e.MenuOption?.Code ?? string.Empty,
                e.MenuOption?.Name ?? string.Empty))
            .ToList();
    }
}
