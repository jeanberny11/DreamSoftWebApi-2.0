using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.GetSubscriptionPlans;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand(
    int             Id,
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    int             TierLevel,
    int             TrialDays,
    bool            IsActive) : IRequest<SubscriptionPlanDto>;
