using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.CreateSubscriptionPlan;

public record CreateSubscriptionPlanCommand(
    string Code,
    string Name,
    TranslationsDto Translations,
    int SolutionId,
    int TierLevel,
    string? Description = null,
    int TrialDays       = 0) : IRequest<SubscriptionPlanDto>;
