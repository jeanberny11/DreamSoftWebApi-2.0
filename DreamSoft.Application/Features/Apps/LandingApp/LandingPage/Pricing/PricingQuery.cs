using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing;
  public record PricingQuery(string? Language = null) : IRequest<List<PricingResponse>>;
