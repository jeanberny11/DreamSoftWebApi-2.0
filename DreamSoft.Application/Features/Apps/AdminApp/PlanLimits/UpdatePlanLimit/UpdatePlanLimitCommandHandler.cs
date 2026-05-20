using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.GetPlanLimits;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.UpdatePlanLimit;

public class UpdatePlanLimitCommandHandler(
    IPlanLimitRepository planLimitRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlanLimitCommand, PlanLimitDto>
{
    public async Task<PlanLimitDto> Handle(
        UpdatePlanLimitCommand request, CancellationToken cancellationToken)
    {
        var limit = await planLimitRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "PlanLimit", request.Id);

        limit.UpdateValue(request.LimitValue, request.Description ?? string.Empty);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PlanLimitDto(limit.Id, limit.PlanId, limit.LimitKey, limit.LimitValue, limit.Description);
    }
}
