using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.GetSubscriptionPlans;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record SubscriptionPlanDto(
    int     Id,
    string  Code,
    string  Name,
    string  Description,
    int     SolutionId,
    string  SolutionCode,
    int     TierLevel,
    int     TrialDays,
    bool    IsActive);

// ── Get All ───────────────────────────────────────────────────────────────────

public record GetSubscriptionPlansQuery : IRequest<IReadOnlyList<SubscriptionPlanDto>>;

public class GetSubscriptionPlansQueryHandler(ISubscriptionPlanRepository planRepository)
    : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanDto>>
{
    public async Task<IReadOnlyList<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await planRepository.GetAllAsync(cancellationToken);

        return plans
            .Select(p => new SubscriptionPlanDto(
                p.Id, p.Code, p.Name, p.Description,
                p.SolutionId, p.Solution?.Code ?? string.Empty,
                p.TierLevel, p.TrialDays, p.IsActive))
            .ToList();
    }
}

// ── Get By Solution ───────────────────────────────────────────────────────────

public record GetSubscriptionPlansBySolutionQuery(int SolutionId)
    : IRequest<IReadOnlyList<SubscriptionPlanDto>>;

public class GetSubscriptionPlansBySolutionQueryHandler(
    ISubscriptionPlanRepository planRepository,
    ISolutionRepository solutionRepository)
    : IRequestHandler<GetSubscriptionPlansBySolutionQuery, IReadOnlyList<SubscriptionPlanDto>>
{
    public async Task<IReadOnlyList<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansBySolutionQuery request, CancellationToken cancellationToken)
    {
        var solution = await solutionRepository.GetByIdAsync(request.SolutionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Solution", request.SolutionId);

        var plans = await planRepository.GetBySolutionIdAsync(request.SolutionId, cancellationToken);

        return plans
            .Select(p => new SubscriptionPlanDto(
                p.Id, p.Code, p.Name, p.Description,
                p.SolutionId, solution.Code,
                p.TierLevel, p.TrialDays, p.IsActive))
            .ToList();
    }
}
