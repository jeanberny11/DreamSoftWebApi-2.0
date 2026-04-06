using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SubscriptionPlanCommandHandler(IApplicationDbContext context) : IRequestHandler<SubscriptionPlanRequest, List<SubscriptionPlanDTO>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<List<SubscriptionPlanDTO>> Handle(SubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        var language = request.Language;
        var subscriptionPlans = await _context.SubscriptionPlans
            .Where(s => s.Solution.Code == request.SolutionCode && s.IsActive)
            .Include(s => s.PlanPrices).ThenInclude(pp => pp.BillingCycle)
            .Include(s => s.Solution)
            .ToListAsync(cancellationToken);
        return [.. subscriptionPlans.Select(s => {
            var firstPrice = s.PlanPrices.FirstOrDefault(pp => pp.IsActive);
            return new SubscriptionPlanDTO {
                Code = s.Code,
                Name = s.Translations.GetDescriptionOrFallback(language, s.Description),
                Description = s.Translations.GetDescriptionOrFallback(language, s.Description),
                Icon = s.Solution.Icon,
                SortOrder = s.Solution.SortOrder,
                IsPopular = false,
                BillingCycle = firstPrice?.BillingCycle.Translations.GetNameOrFallback(language, firstPrice.BillingCycle.Name) ?? "",
                Price = firstPrice?.Price ?? 0m,
                TrialDays = s.TrialDays
            };
        })];
    }
}
