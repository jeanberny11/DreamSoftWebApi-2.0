using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.UpdatePlanPrice;

public record UpdatePlanPriceCommand(
    int Id,
    decimal Price,
    bool IsActive) : IRequest<Unit>;
