using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanPrices.DeletePlanPrice;

public record DeletePlanPriceCommand(int Id) : IRequest;

public class DeletePlanPriceCommandHandler(
    IPlanPriceRepository planPriceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlanPriceCommand>
{
    public async Task Handle(DeletePlanPriceCommand request, CancellationToken cancellationToken)
    {
        var planPrice = await planPriceRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "PlanPrice", request.Id);

        await planPriceRepository.DeleteAsync(planPrice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
