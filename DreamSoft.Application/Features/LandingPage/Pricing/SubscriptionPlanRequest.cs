using MediatR;

namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SubscriptionPlanRequest : IRequest<List<SubscriptionPlanDTO>>
{
    public string SolutionCode { get; set; } = null!;
    public string Language { get; set; } = null!;
}
