using System.Text.Json;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DreamSoft.Infrastructure.Services.Features.Caching;

/// <summary>
/// Service for Redis cache operations (OTP codes, rate limiting)
/// </summary>
public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RedisService> _logger;
    private readonly int _maxEmailSendPerHour;
    private readonly int _maxVerificationAttempts;

    public RedisService(
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<RedisService> logger)
    {
        _redis = redis;
        _configuration = configuration;
        _logger = logger;

        _maxEmailSendPerHour = int.Parse(configuration["RateLimit:MaxEmailSendPerHour"] ?? "3");
        _maxVerificationAttempts = int.Parse(configuration["RateLimit:MaxVerificationAttemptsPerCode"] ?? "5");
    }

    /// <summary>
    /// Stores email verification code in Redis with 5-minute expiration
    /// Key: email_verification:{email}
    /// Value: JSON { code, attempts, createdAt }
    /// </summary>
    public async Task SetEmailVerificationCodeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"email_verification:{email.ToLowerInvariant()}";

            var verificationData = new EmailVerificationData
            {
                Code = code,
                Attempts = 0,
                CreatedAt = DateTime.UtcNow
            };

            var jsonData = JsonSerializer.Serialize(verificationData);
            var expiration = TimeSpan.FromMinutes(5); // 5 minutes

            await db.StringSetAsync(key, jsonData, expiration);

            _logger.LogInformation(
                "Verification code stored in Redis for email: {Email}, expires in 5 minutes",
                email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing verification code in Redis for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Retrieves email verification data from Redis
    /// Returns null if not found or expired
    /// </summary>
    public async Task<EmailVerificationData?> GetEmailVerificationDataAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"email_verification:{email.ToLowerInvariant()}";

            var jsonData = await db.StringGetAsync(key);

            if (!jsonData.HasValue)
            {
                _logger.LogDebug("No verification data found in Redis for email: {Email}", email);
                return null;
            }

            var verificationData = JsonSerializer.Deserialize<EmailVerificationData>(jsonData!);

            _logger.LogDebug("Verification data retrieved from Redis for email: {Email}", email);

            return verificationData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving verification data from Redis for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Increments verification attempts counter
    /// Updates the verification data in Redis
    /// </summary>
    public async Task IncrementVerificationAttemptsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"email_verification:{email.ToLowerInvariant()}";

            var jsonData = await db.StringGetAsync(key);

            if (!jsonData.HasValue)
            {
                _logger.LogWarning("Cannot increment attempts - no verification data found for email: {Email}", email);
                return;
            }

            var verificationData = JsonSerializer.Deserialize<EmailVerificationData>(jsonData!);
            verificationData!.Attempts++;

            var updatedJsonData = JsonSerializer.Serialize(verificationData);
            var remainingTtl = await db.KeyTimeToLiveAsync(key);

            // Preserve the original TTL
            await db.StringSetAsync(key, updatedJsonData, (Expiration)remainingTtl);

            _logger.LogDebug(
                "Verification attempts incremented to {Attempts} for email: {Email}",
                verificationData.Attempts, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing verification attempts for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Deletes email verification code from Redis
    /// Called after successful verification
    /// </summary>
    public async Task DeleteEmailVerificationCodeAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"email_verification:{email.ToLowerInvariant()}";

            await db.KeyDeleteAsync(key);

            _logger.LogInformation("Verification code deleted from Redis for email: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting verification code from Redis for email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Checks rate limit for email sending (max 3 per hour per IP)
    /// Key: email_send_rate:{ipAddress}
    /// Returns true if within limit, false if exceeded
    /// </summary>
    public async Task<bool> CheckEmailSendRateLimitAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"email_send_rate:{ipAddress}";

            // Get current count
            var currentCount = await db.StringGetAsync(key);

            if (!currentCount.HasValue)
            {
                // First request - set count to 1 with 1 hour expiration
                await db.StringSetAsync(key, "1", TimeSpan.FromHours(1));
                _logger.LogDebug("Email send rate limit initialized for IP: {IpAddress}", ipAddress);
                return true;
            }

            var count = int.Parse(currentCount!);

            if (count >= _maxEmailSendPerHour)
            {
                _logger.LogWarning(
                    "Email send rate limit exceeded for IP: {IpAddress} ({Count}/{Max})",
                    ipAddress, count, _maxEmailSendPerHour);
                return false;
            }

            // Increment count and preserve TTL
            await db.StringIncrementAsync(key);
            _logger.LogDebug(
                "Email send count incremented for IP: {IpAddress} ({Count}/{Max})",
                ipAddress, count + 1, _maxEmailSendPerHour);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email send rate limit for IP: {IpAddress}", ipAddress);
            // On error, allow the request (fail open)
            return true;
        }
    }

    /// <summary>
    /// Checks rate limit for code verification attempts (max 5 per code)
    /// Returns true if within limit, false if exceeded
    /// </summary>
    public async Task<bool> CheckVerificationAttemptsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verificationData = await GetEmailVerificationDataAsync(email, cancellationToken);

            if (verificationData == null)
            {
                _logger.LogDebug("No verification data found for email: {Email}", email);
                return false;
            }

            if (verificationData.Attempts >= _maxVerificationAttempts)
            {
                _logger.LogWarning(
                    "Verification attempts exceeded for email: {Email} ({Attempts}/{Max})",
                    email, verificationData.Attempts, _maxVerificationAttempts);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking verification attempts for email: {Email}", email);
            // On error, deny the request (fail closed for security)
            return false;
        }
    }
}
