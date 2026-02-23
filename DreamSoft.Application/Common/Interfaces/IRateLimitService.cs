namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// IP-based sliding-window rate limiter.
/// Used to prevent OTP resend abuse and other public-endpoint flooding.
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Returns <c>true</c> if the caller is allowed to perform the action;
    /// <c>false</c> when the sliding-window limit has been exceeded.
    /// Each call that returns <c>true</c> consumes one slot.
    /// </summary>
    /// <param name="key">
    /// A bucket key that uniquely identifies the action + caller.
    /// Example: <c>"resend-otp:{ipAddress}"</c>
    /// </param>
    /// <param name="maxAttempts">Maximum allowed attempts within <paramref name="windowMinutes"/>.</param>
    /// <param name="windowMinutes">Sliding window duration in minutes.</param>
    bool IsAllowed(string key, int maxAttempts, int windowMinutes);
}
