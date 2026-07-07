namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record PlanPriceDto(
    int PlanPriceId,
    decimal Price,
    BillingCycleDto BillingCycle
);