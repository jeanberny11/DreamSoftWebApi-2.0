using DreamSoft.Application.Features.Admin.SubscriptionPlans.GetSubscriptionPlans;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.SubscriptionPlans.CreateSubscriptionPlan;

public record CreateSubscriptionPlanCommand(
    string          Code,
    string          Name,
    TranslationsDto Translations,
    int             SolutionId,
    int             TierLevel,
    string?         Description = null,
    int             TrialDays   = 0) : IRequest<SubscriptionPlanDto>;
