using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.Subscription.HandleWebhook;

public class HandleWebhookCommandHandler(
    IPaymentGateway paymentGateway,
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ISubscriptionStatusRepository subscriptionStatusRepository,
    ITenantStatusRepository tenantStatusRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    ITenantSubdomainRepository tenantSubdomainRepository,
    ISubscriptionInvoiceRepository subscriptionInvoiceRepository,
    ISubscriptionPaymentRepository subscriptionPaymentRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ILogger<HandleWebhookCommandHandler> logger)
    : IRequestHandler<HandleWebhookCommand, Unit>
{
    public async Task<Unit> Handle(
        HandleWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var webhookEvent = await paymentGateway.ParseWebhookAsync(
            request.Payload,
            request.Signature,
            cancellationToken);

        switch (webhookEvent.EventType)
        {
            case WebhookEventType.CheckoutCompleted:
                await HandleCheckoutCompletedAsync(webhookEvent, cancellationToken);
                break;

            case WebhookEventType.CheckoutExpired:
                await HandleCheckoutExpiredAsync(webhookEvent, cancellationToken);
                break;

            case WebhookEventType.InvoicePaid:
                await HandleInvoicePaidAsync(webhookEvent, cancellationToken);
                break;

            case WebhookEventType.InvoicePaymentFailed:
                await HandleInvoicePaymentFailedAsync(webhookEvent, cancellationToken);
                break;

            case WebhookEventType.SubscriptionCancelled:
                await HandleSubscriptionCancelledAsync(webhookEvent, cancellationToken);
                break;

            default:
                logger.LogWarning("Received unhandled webhook event type, ignoring.");
                break;
        }

        return Unit.Value;
    }

    // ── CheckoutCompleted ─────────────────────────────────────────────────────

    private async Task HandleCheckoutCompletedAsync(
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhookEvent.GatewaySessionId))
        {
            logger.LogWarning("Webhook checkout.session.completed is missing GatewaySessionId, skipping.");
            return;
        }

        var subscription = await tenantSubscriptionRepository
            .GetByStripeSessionIdAsync(webhookEvent.GatewaySessionId, cancellationToken);

        if (subscription is null)
        {
            logger.LogWarning(
                "Webhook checkout.session.completed: no subscription found for session {SessionId}, skipping.",
                webhookEvent.GatewaySessionId);
            return;
        }

        // Guard: already activated — duplicate webhook delivery
        if (!string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
        {
            logger.LogWarning(
                "Webhook checkout.session.completed: subscription {SubscriptionId} already activated, skipping duplicate.",
                subscription.Id);
            return;
        }

        var tenant = await tenantRepository.GetByStripeCustomerIdAsync(
            webhookEvent.GatewayCustomerId ?? string.Empty, cancellationToken);

        if (tenant is null)
        {
            logger.LogWarning(
                "Webhook checkout.session.completed: no tenant found for Stripe customer {CustomerId}, skipping.",
                webhookEvent.GatewayCustomerId);
            return;
        }

        var plan = await subscriptionPlanRepository
            .GetByIdAsync(subscription.SubscriptionPlanId, cancellationToken);

        if (plan is null)
        {
            logger.LogError(
                "Webhook checkout.session.completed: subscription plan {PlanId} not found in DB.",
                subscription.SubscriptionPlanId);
            return;
        }

        var targetStatusCode = plan.HasTrial()
            ? SubscriptionStatusCodes.Trial
            : SubscriptionStatusCodes.Active;

        var targetStatus = await subscriptionStatusRepository
            .GetByCodeAsync(targetStatusCode, cancellationToken);

        if (targetStatus is null)
        {
            logger.LogError(
                "Webhook checkout.session.completed: subscription status '{StatusCode}' not found in DB.",
                targetStatusCode);
            return;
        }

        var activeTenantStatus = await tenantStatusRepository
            .GetByCodeAsync(TenantStatusCodes.Active, cancellationToken);

        if (activeTenantStatus is null)
        {
            logger.LogError("Webhook checkout.session.completed: tenant status ACTIVE not found in DB.");
            return;
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(webhookEvent.GatewaySubscriptionId))
                subscription.SetStripeSubscriptionId(webhookEvent.GatewaySubscriptionId);

            subscription.UpdateStatus(targetStatus.Id);
            await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);

            if (tenant.Status?.Code == TenantStatusCodes.PendingSubscription)
            {
                tenant.UpdateStatus(activeTenantStatus.Id);
                await tenantRepository.UpdateAsync(tenant, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Webhook checkout.session.completed: failed to activate subscription {SubscriptionId}.",
                subscription.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        logger.LogInformation(
            "Subscription {SubscriptionId} activated as {Status} for tenant {TenantId}.",
            subscription.Id, targetStatusCode, tenant.Id);
    }

    // ── CheckoutExpired ───────────────────────────────────────────────────────
    // Fires when a tenant abandons the Stripe checkout page and the session
    // expires (after 24h). We mark the subscription PAYMENT_FAILED and notify.

    private async Task HandleCheckoutExpiredAsync(
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhookEvent.GatewaySessionId))
        {
            logger.LogWarning("Webhook checkout.session.expired is missing GatewaySessionId, skipping.");
            return;
        }

        var subscription = await tenantSubscriptionRepository
            .GetByStripeSessionIdAsync(webhookEvent.GatewaySessionId, cancellationToken);

        if (subscription is null)
        {
            logger.LogWarning(
                "Webhook checkout.session.expired: no subscription found for session {SessionId}, skipping.",
                webhookEvent.GatewaySessionId);
            return;
        }

        // Only act if still pending — avoid overwriting ACTIVE/TRIAL
        if (subscription.Status.Code != SubscriptionStatusCodes.ProcessingPayment)
        {
            logger.LogWarning(
                "Webhook checkout.session.expired: subscription {SubscriptionId} is already {Status}, skipping.",
                subscription.Id, subscription.Status.Code);
            return;
        }

        var paymentFailedStatus = await subscriptionStatusRepository
            .GetByCodeAsync(SubscriptionStatusCodes.PaymentFailed, cancellationToken);

        if (paymentFailedStatus is null)
        {
            logger.LogError("Webhook checkout.session.expired: subscription status PAYMENT_FAILED not found in DB.");
            return;
        }

        var tenant = await tenantRepository.GetByStripeCustomerIdAsync(
            webhookEvent.GatewayCustomerId ?? string.Empty, cancellationToken);

        if (tenant is null)
        {
            logger.LogWarning(
                "Webhook checkout.session.expired: no tenant found for Stripe customer {CustomerId}.",
                webhookEvent.GatewayCustomerId);
            return;
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            subscription.UpdateStatus(paymentFailedStatus.Id);
            await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        logger.LogInformation(
            "Subscription {SubscriptionId} marked PAYMENT_FAILED — checkout session expired for tenant {TenantId}.",
            subscription.Id, tenant.Id);

        // Send payment failed email — never throws
        await emailService.SendPaymentFailedAsync(
            toEmail:           tenant.Email,
            firstName:         tenant.FirstName,
            companyName:       tenant.CompanyName,
            planName:          subscription.SubscriptionPlan?.Name ?? string.Empty,
            failureReason:     webhookEvent.FailureReason,
            cancellationToken: cancellationToken);
    }

    // ── InvoicePaid ───────────────────────────────────────────────────────────
    // Single source of truth for invoice creation and payment recording.
    // Confirmation email is sent only on the first invoice.paid per subscription.

    private async Task HandleInvoicePaidAsync(
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhookEvent.GatewaySubscriptionId))
        {
            logger.LogWarning("Webhook invoice.paid is missing GatewaySubscriptionId, skipping.");
            return;
        }

        var subscription = await tenantSubscriptionRepository
            .GetByStripeSubscriptionIdAsync(
                webhookEvent.GatewaySubscriptionId, cancellationToken);

        if (subscription is null)
        {
            logger.LogWarning(
                "Webhook invoice.paid: no subscription found for {StripeSubscriptionId}, skipping.",
                webhookEvent.GatewaySubscriptionId);
            return;
        }

        SubscriptionInvoice? invoice = null;

        if (!string.IsNullOrWhiteSpace(webhookEvent.GatewayInvoiceId))
        {
            invoice = await subscriptionInvoiceRepository
                .GetByStripeInvoiceIdAsync(webhookEvent.GatewayInvoiceId, cancellationToken);
        }

        var paidAt = webhookEvent.PaidAt ?? DateTime.UtcNow;
        bool isFirstPayment = false;

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (invoice is null)
            {
                invoice = SubscriptionInvoice.Create(
                    tenantId:             subscription.TenantId,
                    tenantSubscriptionId: subscription.Id,
                    amount:               webhookEvent.AmountPaid ?? 0m,
                    currency:             webhookEvent.Currency ?? "USD",
                    dueDate:              paidAt,
                    stripeInvoiceId:      webhookEvent.GatewayInvoiceId);

                invoice.MarkAsPaid(paidAt);
                await subscriptionInvoiceRepository.AddAsync(invoice, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                isFirstPayment = true;
            }
            else
            {
                isFirstPayment = invoice.Status == "unpaid";
                invoice.MarkAsPaid(paidAt);
                await subscriptionInvoiceRepository.UpdateAsync(invoice, cancellationToken);
            }

            invoice.UpdateStripeDetails(
                hostedInvoiceUrl: webhookEvent.HostedInvoiceUrl,
                invoicePdfUrl:    webhookEvent.InvoicePdfUrl,
                invoiceNumber:    webhookEvent.InvoiceNumber,
                billingReason:    webhookEvent.BillingReason);
            await subscriptionInvoiceRepository.UpdateAsync(invoice, cancellationToken);

            var payment = SubscriptionPayment.Create(
                subscriptionInvoiceId: invoice.Id,
                tenantId:              subscription.TenantId,
                amount:                webhookEvent.AmountPaid ?? 0m,
                currency:              webhookEvent.Currency ?? "USD",
                status:                "paid",
                paymentDate:           paidAt,
                stripePaymentId:       webhookEvent.GatewayInvoiceId);

            await subscriptionPaymentRepository.AddAsync(payment, cancellationToken);

            // Restore PAST_DUE subscriptions to ACTIVE on successful renewal
            if (subscription.Status?.Code == SubscriptionStatusCodes.PastDue)
            {
                var activeStatus = await subscriptionStatusRepository
                    .GetByCodeAsync(SubscriptionStatusCodes.Active, cancellationToken);

                if (activeStatus is not null)
                {
                    subscription.UpdateStatus(activeStatus.Id);
                    await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);

                    logger.LogInformation(
                        "Subscription {SubscriptionId} restored from PAST_DUE to ACTIVE after successful renewal.",
                        subscription.Id);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Webhook invoice.paid: failed to process payment for subscription {SubscriptionId}.",
                subscription.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        logger.LogInformation(
            "Invoice {InvoiceId} paid — amount {Amount} {Currency} recorded for subscription {SubscriptionId}.",
            invoice.Id, invoice.Amount, invoice.Currency, subscription.Id);

        if (isFirstPayment)
            await SendSubscriptionConfirmationEmailAsync(subscription, invoice, paidAt, cancellationToken);
    }

    // ── InvoicePaymentFailed ──────────────────────────────────────────────────

    private async Task HandleInvoicePaymentFailedAsync(
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhookEvent.GatewaySubscriptionId))
        {
            logger.LogWarning("Webhook invoice.payment_failed is missing GatewaySubscriptionId, skipping.");
            return;
        }

        var subscription = await tenantSubscriptionRepository
            .GetByStripeSubscriptionIdAsync(
                webhookEvent.GatewaySubscriptionId, cancellationToken);

        if (subscription is null)
        {
            logger.LogWarning(
                "Webhook invoice.payment_failed: no subscription found for {StripeSubscriptionId}, skipping.",
                webhookEvent.GatewaySubscriptionId);
            return;
        }

        var pastDueStatus = await subscriptionStatusRepository
            .GetByCodeAsync(SubscriptionStatusCodes.PastDue, cancellationToken);

        if (pastDueStatus is null)
        {
            logger.LogError("Webhook invoice.payment_failed: subscription status PAST_DUE not found in DB.");
            return;
        }

        var tenant = await tenantRepository.GetByIdAsync(
            subscription.TenantId, cancellationToken);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            subscription.UpdateStatus(pastDueStatus.Id);
            await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);

            if (!string.IsNullOrWhiteSpace(webhookEvent.GatewayInvoiceId))
            {
                var invoice = await subscriptionInvoiceRepository
                    .GetByStripeInvoiceIdAsync(webhookEvent.GatewayInvoiceId, cancellationToken);

                if (invoice is not null)
                {
                    invoice.MarkAsFailed();
                    await subscriptionInvoiceRepository.UpdateAsync(invoice, cancellationToken);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        logger.LogInformation(
            "Subscription {SubscriptionId} marked PAST_DUE after payment failure for tenant {TenantId}.",
            subscription.Id, subscription.TenantId);

        // Notify tenant — never throws
        if (tenant is not null)
        {
            await emailService.SendPaymentFailedAsync(
                toEmail:           tenant.Email,
                firstName:         tenant.FirstName,
                companyName:       tenant.CompanyName,
                planName:          subscription.SubscriptionPlan?.Name ?? string.Empty,
                failureReason:     webhookEvent.FailureReason,
                cancellationToken: cancellationToken);
        }
    }

    // ── SubscriptionCancelled ─────────────────────────────────────────────────

    private async Task HandleSubscriptionCancelledAsync(
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhookEvent.GatewaySubscriptionId))
        {
            logger.LogWarning("Webhook customer.subscription.deleted is missing GatewaySubscriptionId, skipping.");
            return;
        }

        var subscription = await tenantSubscriptionRepository
            .GetByStripeSubscriptionIdAsync(
                webhookEvent.GatewaySubscriptionId, cancellationToken);

        if (subscription is null)
        {
            logger.LogWarning(
                "Webhook customer.subscription.deleted: no subscription found for {StripeSubscriptionId}, skipping.",
                webhookEvent.GatewaySubscriptionId);
            return;
        }

        var cancelledStatus = await subscriptionStatusRepository
            .GetByCodeAsync(SubscriptionStatusCodes.Cancelled, cancellationToken);

        if (cancelledStatus is null)
        {
            logger.LogError("Webhook customer.subscription.deleted: subscription status CANCELLED not found in DB.");
            return;
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            subscription.UpdateStatus(cancelledStatus.Id);
            subscription.Cancel(DateTime.UtcNow);
            await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        logger.LogInformation(
            "Subscription {SubscriptionId} cancelled for tenant {TenantId}.",
            subscription.Id, subscription.TenantId);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Sends the subscription confirmation email on first invoice.paid.
    /// Never throws — a failed email must not affect the already-committed subscription.
    /// </summary>
    private async Task SendSubscriptionConfirmationEmailAsync(
        TenantSubscription subscription,
        SubscriptionInvoice invoice,
        DateTime paidAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await tenantRepository.GetByIdAsync(
                subscription.TenantId, cancellationToken);

            if (tenant is null)
            {
                logger.LogWarning(
                    "Subscription confirmation email skipped: tenant {TenantId} not found.",
                    subscription.TenantId);
                return;
            }

            var subdomain = await tenantSubdomainRepository
                .GetByTenantAndSolutionAsync(subscription.TenantId, subscription.SolutionId, cancellationToken);

            if (subdomain is null)
            {
                logger.LogWarning(
                    "Subscription confirmation email skipped: subdomain not found for tenant {TenantId}.",
                    subscription.TenantId);
                return;
            }

            var planName     = subscription.SubscriptionPlan?.Name ?? string.Empty;
            var billingCycle = subscription.PlanPrice?.BillingCycle?.Name ?? string.Empty;

            await emailService.SendSubscriptionConfirmationAsync(
                toEmail:       tenant.Email,
                firstName:     tenant.FirstName,
                companyName:   tenant.CompanyName,
                subdomain:     subdomain.Subdomain,
                planName:      planName,
                billingCycle:  billingCycle,
                amount:        invoice.Amount,
                currency:      invoice.Currency,
                invoiceNumber: invoice.StripeInvoiceId ?? invoice.Id.ToString(),
                paidAt:        paidAt,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send subscription confirmation email for tenant {TenantId}.",
                subscription.TenantId);
        }
    }
}
