using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.GetPlanLimits;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.CreatePlanLimit;

public class CreatePlanLimitCommandHandler(
    IPlanLimitRepository planLimitRepository,
    ISubscriptionPlanRepository planRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePlanLimitCommand, PlanLimitDto>
{
    public async Task<PlanLimitDto> Handle(
        CreatePlanLimitCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.PlanId);

        var duplicate = await planLimitRepository.GetByPlanAndKeyAsync(
            request.PlanId, request.LimitKey, cancellationToken);

        if (duplicate is not null)
            throw new ConflictException("PlanLimitKeyAlreadyExists",
                $"Plan {request.PlanId} already has a limit with key '{request.LimitKey}'");

        var limit = PlanLimit.Create(
            request.PlanId,
            request.LimitKey,
            request.LimitValue,
            request.Description ?? string.Empty);

        await planLimitRepository.AddAsync(limit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PlanLimitDto(limit.Id, limit.PlanId, limit.LimitKey, limit.LimitValue, limit.Description);
    }
}
