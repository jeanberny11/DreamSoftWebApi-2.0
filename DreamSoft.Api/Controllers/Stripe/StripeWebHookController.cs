using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.HandleWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Api.Controllers.Stripe;

[AllowAnonymous]
public class StripeWebhookController(
    IPaymentGateway paymentGateway,
    IWebhookEventStore eventStore,
    ISender mediator,
    ILogger<StripeWebhookController> logger) : ApiControllerBase
{
    // Checkout-flow events that must be collected and executed in order.
    // All other events are standalone and processed immediately.
    private static readonly HashSet<string> CheckoutFlowEvents =
    [
        nameof(WebhookEventType.CheckoutCompleted),
        nameof(WebhookEventType.InvoicePaid)
    ];

    /// <summary>
    /// Stripe webhook endpoint.
    ///
    /// Checkout-flow events (checkout.session.completed + invoice.paid):
    ///   Stored in Redis grouped by StripeSubscriptionId. Once both have
    ///   arrived they are executed in the correct order then deleted from Redis.
    ///
    /// Standalone events (expired, payment_failed, subscription.deleted):
    ///   Processed immediately — they have no ordering dependency.
    ///
    /// Must NOT use [Authorize] — Stripe calls this without a JWT.
    /// Must NOT use [FromBody] — raw body must be preserved for signature validation.
    /// Always returns 200 so Stripe does not retry.
    /// </summary>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        string payload;
        using (var reader = new StreamReader(Request.Body))
            payload = await reader.ReadToEndAsync(cancellationToken);

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(signature))
            return BadRequest("Missing Stripe-Signature header.");

        // Verify signature and extract normalized event metadata.
        // Throws StripeException on invalid/tampered payloads (caught by middleware → 400).
        var parsed    = await paymentGateway.ParseWebhookAsync(payload, signature, cancellationToken);
        var eventType = parsed.EventType.ToString();

        // ── Standalone events — process immediately ───────────────────────────
        if (!CheckoutFlowEvents.Contains(eventType))
        {
            await mediator.Send(new HandleWebhookCommand(payload, signature), cancellationToken);
            return Ok();
        }

        // ── Checkout-flow events — collect, then execute in order ─────────────

        // Guard: checkout.session.completed may not carry a SubscriptionId on
        // free-trial plans where Stripe creates the subscription asynchronously.
        // Fall back to immediate processing in that edge case.
        if (string.IsNullOrWhiteSpace(parsed.GatewaySubscriptionId))
        {
            logger.LogWarning(
                "Webhook {EventType} received without GatewaySubscriptionId — falling back to immediate processing.",
                eventType);
            await mediator.Send(new HandleWebhookCommand(payload, signature), cancellationToken);
            return Ok();
        }

        var isComplete = await eventStore.StoreAndCheckCompleteAsync(
            parsed.GatewaySubscriptionId,
            eventType,
            payload,
            signature,
            cancellationToken);

        if (!isComplete)
            return Ok();

        // All required events are present — execute in dependency order.
        var orderedEvents = await eventStore.GetOrderedEventsAsync(
            parsed.GatewaySubscriptionId, cancellationToken);

        foreach (var evt in orderedEvents)
        {
            await mediator.Send(
                new HandleWebhookCommand(evt.Payload, evt.Signature),
                cancellationToken);
        }

        await eventStore.DeleteAsync(parsed.GatewaySubscriptionId, cancellationToken);

        logger.LogInformation(
            "Checkout-flow webhook sequence completed for subscription {SubscriptionId}.",
            parsed.GatewaySubscriptionId);

        return Ok();
    }
}
