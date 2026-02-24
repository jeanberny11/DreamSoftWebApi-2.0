using System.Collections.Concurrent;
using DreamSoft.Application.Common.Interfaces;

namespace DreamSoft.Infrastructure.Services.RateLimit;

/// <summary>
/// Thread-safe in-memory sliding-window rate limiter.
/// Suitable for single-node deployments.
/// For multi-node deployments, replace with a Redis-backed implementation.
/// </summary>
public sealed class InMemoryRateLimitService : IRateLimitService
{
    // Keyed by bucket key; value is a list of UTC timestamps of recent attempts
    private readonly ConcurrentDictionary<string, List<DateTime>> _buckets = new();

    // Lock used per-bucket to keep sliding-window logic thread-safe
    private readonly object _lock = new();

    /// <inheritdoc />
    public bool IsAllowed(string key, int maxAttempts, int windowMinutes)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes);

        lock (_lock)
        {
            var timestamps = _buckets.GetOrAdd(key, _ => []);

            // Purge expired timestamps (outside the sliding window)
            timestamps.RemoveAll(t => t < cutoff);

            if (timestamps.Count >= maxAttempts)
                return false;

            timestamps.Add(DateTime.UtcNow);
            return true;
        }
    }
}
