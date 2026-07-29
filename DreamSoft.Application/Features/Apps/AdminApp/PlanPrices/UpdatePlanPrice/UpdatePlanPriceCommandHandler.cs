using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.UpdatePlanPrice;

public class UpdatePlanPriceCommandHandler(
    IPlanPriceRepository planPriceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlanPriceCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdatePlanPriceCommand request, CancellationToken cancellationToken)
    {
        var planPrice = await planPriceRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "PlanPrice", request.Id);

        planPrice.UpdatePrice(request.Price);
        planPrice.SetActive(request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
