using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.GetSubscriptionPlanById;

public record GetSubscriptionPlanByIdQuery(int Id) : IRequest<SubscriptionPlanDto>;

public class GetSubscriptionPlanByIdQueryHandler(
    ISubscriptionPlanRepository planRepository,
    ISolutionRepository solutionRepository)
    : IRequestHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDto>
{
    public async Task<SubscriptionPlanDto> Handle(
        GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.Id);

        var solution = await solutionRepository.GetByIdAsync(plan.SolutionId, cancellationToken);

        return new SubscriptionPlanDto(
            plan.Id, plan.Code, plan.Name, plan.Description,
            plan.SolutionId, solution?.Code ?? string.Empty,
            plan.TierLevel, plan.TrialDays, plan.IsActive);
    }
}
