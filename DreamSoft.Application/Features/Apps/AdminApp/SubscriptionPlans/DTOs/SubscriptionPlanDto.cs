namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.DTOs;

public record SubscriptionPlanDto(
    int Id,
    string Code,
    string Name,
    string Description,
    int SolutionId,
    string SolutionCode,
    int TierLevel,
    int TrialDays,
    bool IsActive);
