namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.DTOs;

public record BillingCycleDto(
    int Id,
    string Code,
    string Name,
    string Description,
    int Months,
    bool IsActive);
