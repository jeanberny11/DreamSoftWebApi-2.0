namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.DTOs;

public record PlanLimitDto(
    int Id,
    int PlanId,
    string LimitKey,
    decimal LimitValue,
    string Description);
