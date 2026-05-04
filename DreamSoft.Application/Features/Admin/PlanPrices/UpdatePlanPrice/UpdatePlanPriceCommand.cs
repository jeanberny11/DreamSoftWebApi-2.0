using DreamSoft.Application.Features.Admin.PlanPrices.GetPlanPrices;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanPrices.UpdatePlanPrice;

public record UpdatePlanPriceCommand(
    int     Id,
    decimal Price,
    bool    IsActive) : IRequest<PlanPriceDto>;
