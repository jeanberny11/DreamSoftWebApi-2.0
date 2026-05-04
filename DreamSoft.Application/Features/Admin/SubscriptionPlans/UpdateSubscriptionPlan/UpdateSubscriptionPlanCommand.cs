using DreamSoft.Application.Features.Admin.SubscriptionPlans.GetSubscriptionPlans;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand(
    int             Id,
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    int             TierLevel,
    int             TrialDays,
    bool            IsActive) : IRequest<SubscriptionPlanDto>;
