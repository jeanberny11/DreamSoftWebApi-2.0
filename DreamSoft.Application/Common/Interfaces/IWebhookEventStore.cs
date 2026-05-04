namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Temporary store for incoming Stripe webhook events.
/// Groups events by StripeSubscriptionId so they can be executed
/// in the correct order once all required events have arrived.
/// </summary>
public interface IWebhookEventStore
{
    /// <summary>
    /// Stores an event for the given subscription key.
    /// Returns true if ALL required checkout-flow events have now arrived
    /// and are ready to be executed in order.
    /// </summary>
    Task<bool> StoreAndCheckCompleteAsync(
        string subscriptionId,
        string eventType,
        string payload,
        string signature,
        CancellationToken ct);

    /// <summary>
    /// Retrieves all stored events for a subscription in execution order:
    /// CheckoutCompleted first, then InvoicePaid.
    /// </summary>
    Task<List<StoredWebhookEvent>> GetOrderedEventsAsync(
        string subscriptionId,
        CancellationToken ct);

    /// <summary>
    /// Deletes all stored events for a subscription after successful execution.
    /// </summary>
    Task DeleteAsync(string subscriptionId, CancellationToken ct);
}

/// <summary>A single stored webhook event payload retrieved from the store.</summary>
public record StoredWebhookEvent(string EventType, string Payload, string Signature);
