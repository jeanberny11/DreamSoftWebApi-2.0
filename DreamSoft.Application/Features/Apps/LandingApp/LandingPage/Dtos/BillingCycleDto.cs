namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record BillingCycleDto(
    int BillingCycleId,
    string Code,
    string Name,
    string Description,
    int Months
);
