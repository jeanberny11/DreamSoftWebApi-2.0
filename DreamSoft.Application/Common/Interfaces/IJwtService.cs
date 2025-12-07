using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a session token for email verification flow (30 min expiration)
    /// </summary>
    /// <param name="email">Verified email address</param>
    /// <returns>JWT session token</returns>
    string GenerateSessionToken(string email);

    /// <summary>
    /// Generates an access token after successful registration/login (1 hour expiration)
    /// </summary>
    string GenerateAccessToken(
        int userId,
        int tenantId,
        string email,
        string username,
        bool isAdmin);

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates session token (from email verification step)
    /// </summary>
    /// <returns>Session token data or null if invalid</returns>
    SessionTokenData? ValidateSessionToken(string token);

    /// <summary>
    /// Validates access token
    /// </summary>
    /// <returns>Access token data or null if invalid</returns>
    AccessTokenData? ValidateAccessToken(string token);

    /// <summary>
    /// Validates refresh token from database
    /// </summary>
    /// <exception cref="UnauthorizedException">If token is invalid, expired, or revoked</exception>
    Task<RefreshToken> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data extracted from a valid session token
/// </summary>
public class SessionTokenData
{
    public string Email { get; set; } = null!;
    public bool IsVerified { get; set; }
}

/// <summary>
/// Data extracted from a valid access token
/// </summary>
public class AccessTokenData
{
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public bool IsAdmin { get; set; }
}