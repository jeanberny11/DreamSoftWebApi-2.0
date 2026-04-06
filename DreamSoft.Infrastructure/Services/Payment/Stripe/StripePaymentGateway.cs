using System.Text.Json;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace DreamSoft.Infrastructure.Services.Payment.Stripe;

/// <summary>
/// Stripe implementation of IPaymentGateway.
/// All Stripe-specific concerns are contained here — the Application layer
/// never references Stripe types directly.
/// </summary>
public class StripePaymentGateway(
    IOptions<StripeSettings> settings,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private readonly StripeSettings _settings = settings.Value;
    private readonly ILogger<StripePaymentGateway> _logger = logger;

    // Stripe service clients — instantiated once and reused (they are stateless)
    private readonly CustomerService _customerService = new();
    private readonly SessionService _sessionService = new();
    private readonly SubscriptionService _subscriptionService = new();

    // ── CreateOrGetCustomerAsync ──────────────────────────────────────────

    /// <inheritdoc />
    public async Task<string> CreateOrGetCustomerAsync(
        int tenantId,
        string email,
        string companyName,
        CancellationToken ct = default)
    {
        try
        {
            var searchOptions = new CustomerSearchOptions
            {
                Query = $"metadata['tenant_id']:'{tenantId}'"
            };

            var existing = await _customerService.SearchAsync(searchOptions, cancellationToken: ct);

            if (existing.Data.Count > 0)
            {
                _logger.LogInformation(
                    "Found existing Stripe customer {CustomerId} for tenant {TenantId}",
                    existing.Data[0].Id, tenantId);

                return existing.Data[0].Id;
            }

            var createOptions = new CustomerCreateOptions
            {
                Email = email,
                Name  = companyName,
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId.ToString()
                }
            };

            var customer = await _customerService.CreateAsync(createOptions, cancellationToken: ct);

            _logger.LogInformation(
                "Created Stripe customer {CustomerId} for tenant {TenantId}",
                customer.Id, tenantId);

            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe error while creating/getting customer for tenant {TenantId}", tenantId);
            throw;
        }
    }

    // ── CreateCheckoutSessionAsync ────────────────────────────────────────

    /// <inheritdoc />
    public async Task<CreateCheckoutResult> CreateCheckoutSessionAsync(
        CheckoutRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var subscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = request.TenantId.ToString()
                }
            };

            if (request.TrialDays > 0)
                subscriptionData.TrialPeriodDays = request.TrialDays;

            var options = new SessionCreateOptions
            {
                Customer                = request.GatewayCustomerId,
                Mode                    = "subscription",
                SuccessUrl              = request.SuccessUrl,
                CancelUrl               = request.CancelUrl,
                PaymentMethodCollection = "always",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price    = request.GatewayPriceId,
                        Quantity = 1
                    }
                ],
                SubscriptionData = subscriptionData,
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = request.TenantId.ToString()
                }
            };

            var session = await _sessionService.CreateAsync(options, cancellationToken: ct);

            _logger.LogInformation(
                "Created Stripe checkout session {SessionId} for tenant {TenantId}",
                session.Id, request.TenantId);

            return new CreateCheckoutResult(session.Url, session.Id);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe error creating checkout session for tenant {TenantId}", request.TenantId);
            throw;
        }
    }

    // ── ChangePlanAsync ───────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task ChangePlanAsync(
        ChangePlanRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetAsync(
                request.GatewaySubscriptionId, cancellationToken: ct);

            var subscriptionItemId = subscription.Items.Data[0].Id;

            var updateOptions = new SubscriptionUpdateOptions
            {
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Id    = subscriptionItemId,
                        Price = request.NewGatewayPriceId
                    }
                ],
                ProrationBehavior = request.ProrationImmediate
                    ? "always_invoice"
                    : "none"
            };

            await _subscriptionService.UpdateAsync(
                request.GatewaySubscriptionId, updateOptions, cancellationToken: ct);

            _logger.LogInformation(
                "Changed Stripe subscription {SubscriptionId} to price {PriceId} (proration: {Proration})",
                request.GatewaySubscriptionId, request.NewGatewayPriceId, request.ProrationImmediate);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe error changing plan for subscription {SubscriptionId}",
                request.GatewaySubscriptionId);
            throw;
        }
    }

    // ── CancelSubscriptionAsync ───────────────────────────────────────────

    /// <inheritdoc />
    public async Task CancelSubscriptionAsync(
        string gatewaySubscriptionId,
        bool immediate,
        CancellationToken ct = default)
    {
        try
        {
            if (immediate)
            {
                await _subscriptionService.CancelAsync(
                    gatewaySubscriptionId, cancellationToken: ct);

                _logger.LogInformation(
                    "Immediately cancelled Stripe subscription {SubscriptionId}",
                    gatewaySubscriptionId);
            }
            else
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };

                await _subscriptionService.UpdateAsync(
                    gatewaySubscriptionId, options, cancellationToken: ct);

                _logger.LogInformation(
                    "Scheduled Stripe subscription {SubscriptionId} for cancellation at period end",
                    gatewaySubscriptionId);
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe error cancelling subscription {SubscriptionId}", gatewaySubscriptionId);
            throw;
        }
    }

    // ── ParseWebhookAsync ─────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<PaymentWebhookEvent> ParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload, signature, _settings.WebhookSecret,
                throwOnApiVersionMismatch: false);

            var webhookEvent = stripeEvent.Type switch
            {
                EventTypes.CheckoutSessionCompleted  => HandleCheckoutCompleted(stripeEvent),
                EventTypes.CheckoutSessionExpired    => HandleCheckoutExpired(stripeEvent),
                EventTypes.InvoicePaid               => HandleInvoicePaid(stripeEvent, payload),
                EventTypes.InvoicePaymentFailed      => HandleInvoicePaymentFailed(stripeEvent),
                EventTypes.CustomerSubscriptionDeleted => HandleSubscriptionDeleted(stripeEvent),
                _ => new PaymentWebhookEvent(
                    EventType:             WebhookEventType.Unknown,
                    GatewaySubscriptionId: null,
                    GatewayCustomerId:     null,
                    GatewayInvoiceId:      null,
                    GatewaySessionId:      null,
                    AmountPaid:            null,
                    Currency:              null,
                    PaidAt:                null,
                    TrialDays:             null,
                    FailureReason:         null,
                    HostedInvoiceUrl:      null,
                    InvoicePdfUrl:         null,
                    InvoiceNumber:         null,
                    BillingReason:         null)
            };

            // Fallback: if the SDK failed to deserialize the subscription ID
            // (happens when Stripe API version is newer than Stripe.net expects),
            // extract it directly from the raw JSON payload.
            if (webhookEvent.GatewaySubscriptionId == null &&
                (webhookEvent.EventType == WebhookEventType.InvoicePaid ||
                 webhookEvent.EventType == WebhookEventType.InvoicePaymentFailed))
            {
                var subscriptionIdFromJson = ExtractInvoiceSubscriptionId(payload);
                if (subscriptionIdFromJson != null)
                {
                    webhookEvent = webhookEvent with { GatewaySubscriptionId = subscriptionIdFromJson };
                    _logger.LogDebug(
                        "Extracted GatewaySubscriptionId {SubId} from raw JSON for {EventType}.",
                        subscriptionIdFromJson, webhookEvent.EventType);
                }
            }

            _logger.LogDebug(
                "Parsed Stripe webhook event {StripeType} → {InternalType}",
                stripeEvent.Type, webhookEvent.EventType);

            return Task.FromResult(webhookEvent);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Invalid Stripe webhook signature or payload");
            throw;
        }
    }

    // ── Private mapping helpers ───────────────────────────────────────────

    private static PaymentWebhookEvent HandleCheckoutCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;

        return new PaymentWebhookEvent(
            EventType:             WebhookEventType.CheckoutCompleted,
            GatewaySubscriptionId: session?.SubscriptionId,
            GatewayCustomerId:     session?.CustomerId,
            GatewayInvoiceId:      null,
            GatewaySessionId:      session?.Id,
            AmountPaid:            null,
            Currency:              null,
            PaidAt:                null,
            TrialDays:             null,
            FailureReason:         null,
            HostedInvoiceUrl:      null,
            InvoicePdfUrl:         null,
            InvoiceNumber:         null,
            BillingReason:         null);
    }

    private static PaymentWebhookEvent HandleCheckoutExpired(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;

        return new PaymentWebhookEvent(
            EventType:             WebhookEventType.CheckoutExpired,
            GatewaySubscriptionId: null,
            GatewayCustomerId:     session?.CustomerId,
            GatewayInvoiceId:      null,
            GatewaySessionId:      session?.Id,
            AmountPaid:            null,
            Currency:              null,
            PaidAt:                null,
            TrialDays:             null,
            FailureReason:         "La sesión de pago expiró sin completar el pago.",
            HostedInvoiceUrl:      null,
            InvoicePdfUrl:         null,
            InvoiceNumber:         null,
            BillingReason:         null);
    }

    private static PaymentWebhookEvent HandleInvoicePaid(Event stripeEvent, string payload)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        var invoiceSubscriptionId = invoice?.SubscriptionId ?? invoice?.Subscription?.Id;
        var meta = ExtractInvoiceMetadata(payload);

        return new PaymentWebhookEvent(
            EventType:             WebhookEventType.InvoicePaid,
            GatewaySubscriptionId: invoiceSubscriptionId,
            GatewayCustomerId:     invoice?.CustomerId,
            GatewayInvoiceId:      invoice?.Id,
            GatewaySessionId:      null,
            AmountPaid:            meta.AmountPaid ?? (invoice != null ? invoice.AmountPaid / 100m : null),
            Currency:              invoice?.Currency?.ToUpper(),
            PaidAt:                invoice?.StatusTransitions?.PaidAt,
            TrialDays:             null,
            FailureReason:         null,
            HostedInvoiceUrl:      meta.HostedInvoiceUrl,
            InvoicePdfUrl:         meta.InvoicePdfUrl,
            InvoiceNumber:         meta.InvoiceNumber,
            BillingReason:         meta.BillingReason);
    }

    private static PaymentWebhookEvent HandleInvoicePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        var failedSubscriptionId = invoice?.SubscriptionId ?? invoice?.Subscription?.Id;

        return new PaymentWebhookEvent(
            EventType:             WebhookEventType.InvoicePaymentFailed,
            GatewaySubscriptionId: failedSubscriptionId,
            GatewayCustomerId:     invoice?.CustomerId,
            GatewayInvoiceId:      invoice?.Id,
            GatewaySessionId:      null,
            AmountPaid:            null,
            Currency:              invoice?.Currency?.ToUpper(),
            PaidAt:                null,
            TrialDays:             null,
            FailureReason:         invoice?.LastFinalizationError?.Message,
            HostedInvoiceUrl:      null,
            InvoicePdfUrl:         null,
            InvoiceNumber:         null,
            BillingReason:         null);
    }

    /// <summary>
    /// Extracts the subscription ID from the raw Stripe invoice JSON payload.
    ///
    /// Stripe API 2026-03-25.dahlia+: data.object.parent.subscription_details.subscription
    /// Stripe API pre-2026-03-25:     data.object.subscription (string or expanded object)
    ///
    /// Both paths are tried in order.
    /// </summary>
    private static string? ExtractInvoiceSubscriptionId(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var obj = doc.RootElement
                .GetProperty("data")
                .GetProperty("object");

            // ── New path (API 2026-03-25.dahlia+) ────────────────────────────
            if (obj.TryGetProperty("parent", out var parent) &&
                parent.TryGetProperty("subscription_details", out var subDetails) &&
                subDetails.TryGetProperty("subscription", out var newSubEl))
            {
                var id = newSubEl.ValueKind == JsonValueKind.String
                    ? newSubEl.GetString()
                    : newSubEl.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;

                if (!string.IsNullOrWhiteSpace(id)) return id;
            }

            // ── Legacy path (pre-2026-03-25) ──────────────────────────────────
            if (obj.TryGetProperty("subscription", out var legacySubEl))
            {
                return legacySubEl.ValueKind == JsonValueKind.String
                    ? legacySubEl.GetString()
                    : legacySubEl.TryGetProperty("id", out var legacyIdEl) ? legacyIdEl.GetString() : null;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts invoice metadata fields from the raw Stripe JSON payload:
    /// hosted_invoice_url, invoice_pdf, number, billing_reason.
    /// </summary>
    private static (string? HostedInvoiceUrl, string? InvoicePdfUrl, string? InvoiceNumber, string? BillingReason, decimal? AmountPaid)
        ExtractInvoiceMetadata(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var obj = doc.RootElement
                .GetProperty("data")
                .GetProperty("object");

            string? Get(string field) =>
                obj.TryGetProperty(field, out var el) && el.ValueKind == JsonValueKind.String
                    ? el.GetString()
                    : null;

            decimal? amountPaid = null;
            if (obj.TryGetProperty("amount_paid", out var amountEl) &&
                amountEl.ValueKind == JsonValueKind.Number &&
                amountEl.TryGetInt64(out var amountCents))
            {
                amountPaid = amountCents / 100m;
            }

            return (
                HostedInvoiceUrl: Get("hosted_invoice_url"),
                InvoicePdfUrl:    Get("invoice_pdf"),
                InvoiceNumber:    Get("number"),
                BillingReason:    Get("billing_reason"),
                AmountPaid:       amountPaid
            );
        }
        catch
        {
            return (null, null, null, null, null);
        }
    }

    private static PaymentWebhookEvent HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;

        return new PaymentWebhookEvent(
            EventType:             WebhookEventType.SubscriptionCancelled,
            GatewaySubscriptionId: subscription?.Id,
            GatewayCustomerId:     subscription?.CustomerId,
            GatewayInvoiceId:      null,
            GatewaySessionId:      null,
            AmountPaid:            null,
            Currency:              null,
            PaidAt:                null,
            TrialDays:             null,
            FailureReason:         null,
            HostedInvoiceUrl:      null,
            InvoicePdfUrl:         null,
            InvoiceNumber:         null,
            BillingReason:         null);
    }
}
