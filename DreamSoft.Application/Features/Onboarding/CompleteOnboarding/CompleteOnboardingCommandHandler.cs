using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.Onboarding.CompleteOnboarding;

public class CompleteOnboardingCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CompleteOnboardingCommand, CompleteOnboardingResponse>
{
    public async Task<CompleteOnboardingResponse> Handle(
        CompleteOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant — must be PENDING_SUBSCRIPTION
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        if (tenant.Status.Code != TenantStatusCodes.PendingSubscription)
            throw new ConflictException("OnboardingAlreadyComplete");

        // 2. Load and validate solution
        var solution = await context.Solutions
            .FirstOrDefaultAsync(
                s => s.Id == request.SolutionId && s.IsActive,
                cancellationToken)
            ?? throw new NotFoundException("NotFound", "Solution");

        // 3. Load and validate plan — must be active and belong to the requested solution
        var plan = await context.SubscriptionPlans
            .Include(p => p.BillingCycle)
            .FirstOrDefaultAsync(
                p => p.Id == request.SubscriptionPlanId && p.IsActive,
                cancellationToken)
            ?? throw new NotFoundException("NotFound", "SubscriptionPlan");

        if (plan.SolutionId != request.SolutionId)
            throw new ConflictException("PlanNotBelongToSolution");

        // 4. Determine subscription status — TRIAL if plan has trial days, otherwise ACTIVE
        var statusCode = plan.HasTrial()
            ? SubscriptionStatusCodes.Trial
            : SubscriptionStatusCodes.Active;

        var subscriptionStatus = await context.SubscriptionStatuses
            .FirstOrDefaultAsync(s => s.Code == statusCode, cancellationToken)
            ?? throw new NotFoundException(
                "NotFound", $"SubscriptionStatus {statusCode} not in database.");

        // 5. Build subscription dates
        var startDate = DateTime.UtcNow;
        DateTime? trialEnd = plan.HasTrial()
            ? startDate.AddDays(plan.TrialDays)
            : null;

        var subscription = TenantSubscription.Create(
            tenantId: tenantId,
            solutionId: request.SolutionId,
            subscriptionPlanId: request.SubscriptionPlanId,
            statusId: subscriptionStatus.Id,
            startDate: startDate,
            trialEndDate: trialEnd);

        // 6. Load ACTIVE tenant status
        var activeStatus = await context.TenantStatuses
            .FirstOrDefaultAsync(
                s => s.Code == TenantStatusCodes.Active,
                cancellationToken)
            ?? throw new NotFoundException(
                "NotFound", "TenantStatus ACTIVE not in database.");

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            context.TenantSubscriptions.Add(subscription);
            tenant.TransitionStatus(activeStatus.Id);
            await context.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ─────────────────────────────────────────────

        return new CompleteOnboardingResponse("/dashboard");
    }
}
