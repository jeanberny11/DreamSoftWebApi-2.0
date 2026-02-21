namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SubscriptionPlanDTO
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsPopular { get; set; }
    public string BillingCycle { get; set; } = null!;
    public decimal Price { get; set; }
    public int TrialDays { get; set; }
}