using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing.CheckoutDetail;

public record CheckoutDetailQuery(int PlanId, string? Language = null) : IRequest<CheckoutDetailDto>;
