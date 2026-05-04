using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanLimits.DeletePlanLimit;

public record DeletePlanLimitCommand(int Id) : IRequest;

public class DeletePlanLimitCommandHandler(
    IPlanLimitRepository planLimitRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlanLimitCommand>
{
    public async Task Handle(DeletePlanLimitCommand request, CancellationToken cancellationToken)
    {
        var limit = await planLimitRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "PlanLimit", request.Id);

        await planLimitRepository.DeleteAsync(limit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
