namespace DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.DTOs;

public record PlanPriceDto(
    int Id,
    int PlanId,
    int BillingCycleId,
    string BillingCycleCode,
    string BillingCycleName,
    decimal Price,
    bool IsActive);
