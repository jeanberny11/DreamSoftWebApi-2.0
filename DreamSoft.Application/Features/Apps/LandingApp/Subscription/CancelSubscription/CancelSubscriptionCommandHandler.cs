using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.CancelSubscription;

public class CancelSubscriptionCommandHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ISubscriptionStatusRepository subscriptionStatusRepository,
    ISubscriptionCancellationLogRepository cancellationLogRepository,
    IPaymentGateway paymentGateway,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CancelSubscriptionCommand, CancelSubscriptionResponse>
{
    public async Task<CancelSubscriptionResponse> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 2. Tenant must be ACTIVE
        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new ConflictException("InvalidTenantStatus");

        // 3. Find the subscription for the requested solution
        var subscription = await tenantSubscriptionRepository
            .GetByTenantAndSolutionAsync(tenantId, request.SolutionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.TenantSubscriptionNotFound, tenantId, request.SolutionId);

        // 4. Only ACTIVE, TRIAL or PAST_DUE subscriptions can be cancelled
        var cancellableStatuses = new[]
        {
            SubscriptionStatusCodes.Active,
            SubscriptionStatusCodes.Trial,
            SubscriptionStatusCodes.PastDue
        };

        if (!cancellableStatuses.Contains(subscription.Status.Code))
            throw new ConflictException("SubscriptionNotCancellable");

        // 5. Must have a Stripe subscription ID
        if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
            throw new ConflictException("NoStripeSubscriptionFound");

        // 6. Call the payment gateway to cancel on Stripe
        await paymentGateway.CancelSubscriptionAsync(
            subscription.StripeSubscriptionId,
            request.CancelImmediately,
            cancellationToken);

        // 7. Apply local changes depending on cancellation type
        DateTime? scheduledEndDate = null;

        if (request.CancelImmediately)
        {
            // Immediately cancelled — update status and set EndDate = now
            var cancelledStatus = await subscriptionStatusRepository
                .GetByCodeAsync(SubscriptionStatusCodes.Cancelled, cancellationToken)
                ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionStatus", "Code = CANCELLED");

            subscription.UpdateStatus(cancelledStatus.Id);
            subscription.Cancel(DateTime.UtcNow);
        }
        else
        {
            // Period-end cancellation — fetch the real period end date from Stripe
            // so the local record shows the exact date the subscription will stop.
            scheduledEndDate = await paymentGateway.GetSubscriptionPeriodEndAsync(
                subscription.StripeSubscriptionId,
                cancellationToken);

            // Mark the subscription as pending cancellation.
            // Status stays ACTIVE/TRIAL — the tenant can still use the product.
            // The Stripe webhook (customer.subscription.deleted) will set the final
            // CANCELLED status when the billing period expires.
            subscription.ScheduleCancellation(scheduledEndDate.Value);
        }

        // 8. Write cancellation audit log
        var cancellationLog = SubscriptionCancellationLog.Create(
            tenantSubscriptionId: subscription.Id,
            tenantId:             tenantId,
            cancellationType:     request.CancelImmediately ? "immediate" : "at_period_end",
            cancelledAt:          DateTime.UtcNow,
            scheduledEndDate:     request.CancelImmediately ? DateTime.UtcNow : scheduledEndDate,
            cancellationReason:   request.CancellationReason,
            cancellationFeedback: request.CancellationFeedback);

        await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await cancellationLogRepository.AddAsync(cancellationLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Send cancellation email — never throws
        await emailService.SendSubscriptionCancelledAsync(
            toEmail:          tenant.Email,
            firstName:        tenant.FirstName,
            companyName:      tenant.CompanyName,
            planName:         subscription.SubscriptionPlan?.Name ?? string.Empty,
            cancellationType: request.CancelImmediately ? "immediate" : "at_period_end",
            scheduledEndDate: request.CancelImmediately ? null : scheduledEndDate,
            cancellationToken: cancellationToken);

        var message = request.CancelImmediately
            ? "Subscription cancelled immediately."
            : $"Subscription will be cancelled at the end of the current billing period ({scheduledEndDate:yyyy-MM-dd}).";

        return new CancelSubscriptionResponse(message, scheduledEndDate);
    }
}
