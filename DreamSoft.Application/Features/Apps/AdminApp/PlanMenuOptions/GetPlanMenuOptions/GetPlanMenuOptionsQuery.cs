using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.GetPlanMenuOptions;

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
