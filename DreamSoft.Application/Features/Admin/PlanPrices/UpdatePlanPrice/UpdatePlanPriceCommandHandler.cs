using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.PlanPrices.GetPlanPrices;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanPrices.UpdatePlanPrice;

public class UpdatePlanPriceCommandHandler(
    IPlanPriceRepository planPriceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlanPriceCommand, PlanPriceDto>
{
    public async Task<PlanPriceDto> Handle(
        UpdatePlanPriceCommand request, CancellationToken cancellationToken)
    {
        var planPrice = await planPriceRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "PlanPrice", request.Id);

        planPrice.UpdatePrice(request.Price);
        planPrice.SetActive(request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PlanPriceDto(
            planPrice.Id, planPrice.PlanId,
            planPrice.BillingCycleId,
            planPrice.BillingCycle?.Code ?? string.Empty,
            planPrice.BillingCycle?.Name ?? string.Empty,
            planPrice.Price, planPrice.IsActive);
    }
}
