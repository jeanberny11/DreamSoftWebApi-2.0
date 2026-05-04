using DreamSoft.Application.Features.Admin.PlanLimits.GetPlanLimits;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanLimits.CreatePlanLimit;

public record CreatePlanLimitCommand(
    int     PlanId,
    string  LimitKey,
    decimal LimitValue,
    string? Description = null) : IRequest<PlanLimitDto>;
