using MediatR;

namespace DreamSoft.Application.Features.LandingPage.Pricing;
  public record PricingQuery(string? Language = null) : IRequest<List<PricingResponse>>;
