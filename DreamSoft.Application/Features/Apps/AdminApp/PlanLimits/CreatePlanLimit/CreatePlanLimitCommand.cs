using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.GetPlanLimits;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.CreatePlanLimit;

public record CreatePlanLimitCommand(
    int     PlanId,
    string  LimitKey,
    decimal LimitValue,
    string? Description = null) : IRequest<PlanLimitDto>;
