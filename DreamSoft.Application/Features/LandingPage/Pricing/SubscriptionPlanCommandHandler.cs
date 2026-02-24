using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.LandingPage.Pricing;
public class SubscriptionPlanCommandHandler(IApplicationDbContext context, ILogger<SubscriptionPlanCommandHandler> logger) : IRequestHandler<SubscriptionPlanRequest, List<SubscriptionPlanDTO>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<SubscriptionPlanCommandHandler> _logger = logger;
    public async Task<List<SubscriptionPlanDTO>> Handle(SubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        var language = request.Language;
        _logger.LogInformation(
            "Subscription plan requested with language: {Language}",
            language);
        var subscriptionPlans = await _context.SubscriptionPlans.Where(s => s.Solution.Code == request.SolutionCode && s.IsActive).Include(s => s.BillingCycle).Include(s => s.Solution).ToListAsync(cancellationToken);
        return [.. subscriptionPlans.Select(s => new SubscriptionPlanDTO { Code = s.Code, Name = s.Translations.GetDescriptionOrFallback(language,s.Description), 
        Description = s.Translations.GetDescriptionOrFallback(language,s.Description),
         Icon = s.Solution.Icon, SortOrder = s.Solution.SortOrder,
         IsPopular = false,
         BillingCycle = s.BillingCycle.Translations.GetNameOrFallback(language,s.BillingCycle.Name), Price = s.Price, TrialDays = s.TrialDays })];
    }
}