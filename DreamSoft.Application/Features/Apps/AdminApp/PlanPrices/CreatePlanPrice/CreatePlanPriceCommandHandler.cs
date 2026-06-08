using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.CreatePlanPrice;

public class CreatePlanPriceCommandHandler(
    IPlanPriceRepository planPriceRepository,
    ISubscriptionPlanRepository planRepository,
    IBillingCycleRepository billingCycleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePlanPriceCommand, PlanPriceDto>
{
    public async Task<PlanPriceDto> Handle(
        CreatePlanPriceCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.PlanId);

        var billingCycle = await billingCycleRepository.GetByIdAsync(request.BillingCycleId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "BillingCycle", request.BillingCycleId);

        var duplicate = await planPriceRepository.AnyAsync(
            p => p.PlanId == request.PlanId && p.BillingCycleId == request.BillingCycleId,
            cancellationToken);

        if (duplicate)
            throw new ConflictException(ErrorMessageKeys.PlanPriceAlreadyExists,
                $"Plan {request.PlanId} already has a price for billing cycle {request.BillingCycleId}");

        var planPrice = PlanPrice.Create(request.PlanId, request.BillingCycleId, request.Price);

        await planPriceRepository.AddAsync(planPrice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PlanPriceDto(
            planPrice.Id, planPrice.PlanId,
            planPrice.BillingCycleId,
            billingCycle.Code,
            billingCycle.Name,
            planPrice.Price, planPrice.IsActive);
    }
}
