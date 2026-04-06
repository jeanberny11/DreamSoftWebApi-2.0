using System.Text.Json;
using DreamSoft.Application.Common.Interfaces;
using StackExchange.Redis;

// WebhookEventType is defined in DreamSoft.Application.Common.Interfaces (IPaymentGateway.cs)

namespace DreamSoft.Infrastructure.Services.Payment;

/// <summary>
/// Redis-backed implementation of IWebhookEventStore.
///
/// Events are stored as fields in a Redis Hash keyed by StripeSubscriptionId.
/// A 5-minute TTL ensures orphaned keys are cleaned up automatically if
/// something goes wrong before all events arrive.
///
/// Once all required events are present the caller executes them in order
/// and calls DeleteAsync — no data is retained after processing.
/// </summary>
public sealed class RedisWebhookEventStore(IConnectionMultiplexer redis) : IWebhookEventStore
{
    // The event types that must ALL be present before execution.
    // Order here defines the execution order in GetOrderedEventsAsync.
    private static readonly string[] RequiredEvents =
    [
        nameof(WebhookEventType.CheckoutCompleted),
        nameof(WebhookEventType.InvoicePaid)
    ];

    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(300);

    private static string Key(string subscriptionId) =>
        $"webhook:checkout:{subscriptionId}";

    /// <inheritdoc />
    public async Task<bool> StoreAndCheckCompleteAsync(
        string subscriptionId,
        string eventType,
        string payload,
        string signature,
        CancellationToken ct)
    {
        var db  = redis.GetDatabase();
        var key = Key(subscriptionId);

        // Wrap payload + signature together so we can retrieve both later
        var value = JsonSerializer.Serialize(new { payload, signature });

        await db.HashSetAsync(key, eventType, value);
        await db.KeyExpireAsync(key, Ttl);

        // Check whether all required events are now present
        var fields  = await db.HashKeysAsync(key);
        var present = fields.Select(f => f.ToString()).ToHashSet();
        var isComplete = RequiredEvents.All(e => present.Contains(e));

        return isComplete;
    }

    /// <inheritdoc />
    public async Task<List<StoredWebhookEvent>> GetOrderedEventsAsync(
        string subscriptionId,
        CancellationToken ct)
    {
        var db     = redis.GetDatabase();
        var key    = Key(subscriptionId);
        var result = new List<StoredWebhookEvent>();

        // Iterate in RequiredEvents order — this enforces execution order
        foreach (var eventType in RequiredEvents)
        {
            var raw = await db.HashGetAsync(key, eventType);
            if (!raw.HasValue) continue;

            var wrapper = JsonSerializer.Deserialize<JsonElement>(raw!);
            result.Add(new StoredWebhookEvent(
                EventType: eventType,
                Payload:   wrapper.GetProperty("payload").GetString()!,
                Signature: wrapper.GetProperty("signature").GetString()!));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string subscriptionId, CancellationToken ct)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(Key(subscriptionId));
    }
}
