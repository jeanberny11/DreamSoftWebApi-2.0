using DreamSoft.Application.Features.Admin.PlanLimits.GetPlanLimits;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanLimits.UpdatePlanLimit;

public record UpdatePlanLimitCommand(
    int     Id,
    decimal LimitValue,
    string? Description = null) : IRequest<PlanLimitDto>;
