using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.UpdatePlanLimit;

public record UpdatePlanLimitCommand(
    int Id,
    decimal LimitValue,
    string? Description = null) : IRequest<Unit>;
