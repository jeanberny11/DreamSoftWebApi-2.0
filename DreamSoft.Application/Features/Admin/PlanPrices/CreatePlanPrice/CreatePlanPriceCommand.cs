using DreamSoft.Application.Features.Admin.PlanPrices.GetPlanPrices;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanPrices.CreatePlanPrice;

public record CreatePlanPriceCommand(
    int     PlanId,
    int     BillingCycleId,
    decimal Price) : IRequest<PlanPriceDto>;
