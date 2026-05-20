using DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.GetPlanPrices;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.CreatePlanPrice;

public record CreatePlanPriceCommand(
    int     PlanId,
    int     BillingCycleId,
    decimal Price) : IRequest<PlanPriceDto>;
