namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service for Redis cache operations (OTP codes, rate limiting)
/// </summary>
public interface IRedisService
{
    /// <summary>
    /// Stores email verification code in Redis with 5-minute expiration
    /// </summary>
    Task SetEmailVerificationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves email verification data from Redis
    /// </summary>
    /// <returns>Verification data or null if not found/expired</returns>
    Task<EmailVerificationData?> GetEmailVerificationDataAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments verification attempts counter
    /// </summary>
    Task IncrementVerificationAttemptsAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes email verification code from Redis
    /// </summary>
    Task DeleteEmailVerificationCodeAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks rate limit for email sending (max 3 per hour per IP)
    /// </summary>
    /// <returns>True if within rate limit, false if exceeded</returns>
    Task<bool> CheckEmailSendRateLimitAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks rate limit for code verification attempts (max 5 per code)
    /// </summary>
    /// <returns>True if within rate limit, false if exceeded</returns>
    Task<bool> CheckVerificationAttemptsAsync(
        string email,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Email verification data stored in Redis
/// </summary>
public class EmailVerificationData
{
    public string Code { get; set; } = null!;
    public int Attempts { get; set; }
    public DateTime CreatedAt { get; set; }
}