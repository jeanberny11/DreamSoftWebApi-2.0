using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing.CheckoutDetail;

public class CheckoutDetailQueryHandler(ISubscriptionPlanRepository subscriptionPlanRepository)
    : IRequestHandler<CheckoutDetailQuery, CheckoutDetailDto>
{
    public async Task<CheckoutDetailDto> Handle(
        CheckoutDetailQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var plan = await subscriptionPlanRepository
            .GetWithSolutionPricesAndLimitsAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.SubscriptionPlanNotFound, request.PlanId);

        if (!plan.IsActive)
            throw new ConflictException(ErrorMessageKeys.SubscriptionPlanNotActive);

        return new CheckoutDetailDto(
            SolutionId: plan.Solution.Id,
            SolutionCode: plan.Solution.Code,
            SolutionName: plan.Solution.GetTranslatedName(language),
            SolutionIcon: plan.Solution.Icon,
            PlanId: plan.Id,
            PlanCode: plan.Code,
            PlanName: plan.GetTranslatedName(language),
            PlanDescription: plan.Translations.GetDescriptionOrFallback(language, plan.Description),
            TrialDays: plan.TrialDays,
            Limits: [.. plan.PlanLimits
                .Select(limit => new PlanLimitDto(
                    PlanLimitId: limit.Id,
                    LimitKey: limit.LimitKey,
                    LimitValue: (int)limit.LimitValue,
                    Description: limit.Description))],
            Prices: [.. plan.PlanPrices
                .Where(price => price.IsActive)
                .Select(price => new PlanPriceDto(
                    PlanPriceId: price.Id,
                    Price: price.Price,
                    BillingCycle: new BillingCycleDto(
                        BillingCycleId: price.BillingCycle.Id,
                        Code: price.BillingCycle.Code,
                        Name: price.BillingCycle.Translations.GetNameOrFallback(language, price.BillingCycle.Name),
                        Description: price.BillingCycle.Translations.GetDescriptionOrFallback(language, price.BillingCycle.Description),
                        Months: price.BillingCycle.Months)))]);
    }
}
