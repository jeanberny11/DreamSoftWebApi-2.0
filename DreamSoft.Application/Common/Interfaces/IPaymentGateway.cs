namespace DreamSoft.Application.Common.Interfaces;

// ── DTOs ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Input for creating a hosted checkout session.
/// </summary>
public record CheckoutRequest(
    string GatewayCustomerId,   // e.g. Stripe customer ID
    string GatewayPriceId,      // e.g. Stripe price ID (from PlanPrice.StripePriceId)
    int TenantId,
    int TrialDays,              // 0 = no trial, >0 = bill after N days
    string SuccessUrl,          // frontend redirect on success
    string CancelUrl            // frontend redirect on cancel
);

/// <summary>
/// Result returned after creating a checkout session.
/// </summary>
public record CreateCheckoutResult(
    string CheckoutUrl,         // redirect the frontend here
    string GatewaySessionId     // stored for reconciliation on webhook
);

/// <summary>
/// Input for upgrading or downgrading an existing subscription.
/// </summary>
public record ChangePlanRequest(
    string GatewaySubscriptionId,
    string NewGatewayPriceId,
    bool ProrationImmediate,    // true = charge/credit immediately, false = next billing date
    // When set, pins the actual charge to match a previously shown preview
    // exactly (Stripe's own recommendation — see PreviewPlanChangeAsync).
    DateTime? ProrationDate = null
);

/// <summary>
/// Result of previewing a plan change before committing it.
/// ProrationDate MUST be echoed back on the subsequent ChangePlanRequest so
/// the real charge matches this preview exactly.
/// </summary>
public record PlanChangePreviewResult(
    decimal CreditAmount,   // unused-time credit on the current price; ≤ 0
    decimal ChargeAmount,   // prorated charge for the new price; ≥ 0
    decimal AmountDueNow,   // net total Stripe would actually charge today
    string Currency,
    DateTime ProrationDate
);

/// <summary>
/// Normalised event types received from the payment gateway webhook.
/// Each value maps to a specific gateway event (e.g. Stripe event name).
/// </summary>
public enum WebhookEventType
{
    CheckoutCompleted,          // checkout.session.completed
    CheckoutExpired,            // checkout.session.expired
    InvoicePaid,                // invoice.paid
    InvoicePaymentFailed,       // invoice.payment_failed
    SubscriptionCancelled,      // customer.subscription.deleted
    Unknown
}

/// <summary>
/// Normalised webhook event — gateway-agnostic representation passed to the handler.
/// </summary>
public record PaymentWebhookEvent(
    WebhookEventType EventType,
    string? GatewaySubscriptionId,
    string? GatewayCustomerId,
    string? GatewayInvoiceId,
    string? GatewaySessionId,
    decimal? AmountPaid,
    string? Currency,
    DateTime? PaidAt,
    int? TrialDays,
    string? FailureReason,
    string? HostedInvoiceUrl,
    string? InvoicePdfUrl,
    string? InvoiceNumber,
    string? BillingReason,
    string? GatewayPaymentIntentId = null,  // Stripe PaymentIntent — the actual charge attempt
    decimal? AmountDue = null               // invoice amount_due — known even when payment fails
);

// ── Interface ─────────────────────────────────────────────────────────────────

/// <summary>
/// Payment gateway abstraction. Implement this interface to support a new
/// payment provider (e.g. PayPal, Mercado Pago) without touching Application logic.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Creates a customer in the gateway for the given tenant if one does not
    /// already exist, or returns the existing gateway customer ID.
    /// The caller is responsible for persisting the returned ID on the Tenant.
    /// </summary>
    Task<string> CreateOrGetCustomerAsync(
        int tenantId,
        string email,
        string companyName,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a hosted checkout session and returns the redirect URL plus
    /// the gateway session ID. The frontend redirects the user to CheckoutUrl.
    /// The actual subscription is created asynchronously via webhook.
    /// </summary>
    Task<CreateCheckoutResult> CreateCheckoutSessionAsync(
        CheckoutRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Upgrades or downgrades an existing active subscription to a new plan/price.
    /// ProrationImmediate controls whether the price difference is billed right away.
    /// </summary>
    Task ChangePlanAsync(
        ChangePlanRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Previews the invoice Stripe would generate for a plan change, without
    /// applying it or creating any real invoice. No trial parameters are
    /// ever sent — omitting trial_end preserves any remaining trial days on
    /// the subscription untouched (Stripe only resets billing when a trial
    /// explicitly starts or ends).
    /// </summary>
    Task<PlanChangePreviewResult> PreviewPlanChangeAsync(
        string gatewaySubscriptionId,
        string newGatewayPriceId,
        bool prorationImmediate,
        CancellationToken ct = default);

    /// <summary>
    /// Cancels a subscription. If immediate is false the subscription remains
    /// active until the end of the current billing period.
    /// </summary>
    Task CancelSubscriptionAsync(
        string gatewaySubscriptionId,
        bool immediate,
        CancellationToken ct = default);

    /// <summary>
    /// Reverts a pending period-end cancellation, keeping the subscription
    /// active and renewing. No-op semantics if nothing was scheduled.
    /// </summary>
    Task ResumeSubscriptionAsync(
        string gatewaySubscriptionId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the current billing period end date (UTC) for the given subscription.
    /// Used when scheduling a period-end cancellation so the local record
    /// reflects the exact date the subscription will stop.
    /// </summary>
    Task<DateTime> GetSubscriptionPeriodEndAsync(
        string gatewaySubscriptionId,
        CancellationToken ct = default);

    /// <summary>
    /// Parses and validates the raw webhook payload and signature header.
    /// Throws an exception if the signature is invalid or the payload is malformed.
    /// Returns a normalised PaymentWebhookEvent ready for the handler to process.
    /// </summary>
    Task<PaymentWebhookEvent> ParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default);
}
