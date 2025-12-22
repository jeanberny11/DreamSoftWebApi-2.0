using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DreamSoft.Infrastructure.Services.Features.Authentication;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<JwtService> _logger;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _sessionTokenExpirationMinutes;

    public JwtService(
        IConfiguration configuration,
        IApplicationDbContext context,
        ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;

        // Load JWT configuration
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured in appsettings.json");

        if (_secret.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret must be at least 32 characters long");
        }

        _issuer = configuration["Jwt:Issuer"] ?? "DreamSoftAPI";
        _audience = configuration["Jwt:Audience"] ?? "DreamSoftClient";
        _accessTokenExpirationMinutes = int.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        _sessionTokenExpirationMinutes = int.Parse(configuration["Jwt:SessionTokenExpirationMinutes"] ?? "30");
    }

    /// <summary>
    /// Generates a session token for email verification flow (30 min expiration)
    /// Contains: email, token_type=session, is_verified=true
    /// </summary>
    public string GenerateSessionToken(string email)
    {
        var claims = new[]
        {
            new Claim("email", email.ToLowerInvariant()),
            new Claim("token_type", "session"),
            new Claim("is_verified", "true")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_sessionTokenExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Session token generated for email: {Email}, expires in {Minutes} minutes",
            email, _sessionTokenExpirationMinutes);

        return tokenString;
    }

    /// <summary>
    /// Generates an access token after successful registration/login (1 hour expiration)
    /// Contains: userId, tenantId, email, username, isAdmin, token_type=access
    /// </summary>
    public string GenerateAccessToken(
        int userId,
        int tenantId,
        string email,
        string username,
        bool isAdmin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("email", email.ToLowerInvariant()),
            new Claim("username", username.ToLowerInvariant()),
            new Claim("is_admin", isAdmin.ToString().ToLower()),
            new Claim("token_type", "access")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Access token generated for UserId: {UserId}, TenantId: {TenantId}, Username: {Username}",
            userId, tenantId, username);

        return tokenString;
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token (random 64-byte base64 string)
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);

        _logger.LogDebug("Refresh token generated");

        return refreshToken;
    }

    /// <summary>
    /// Validates session token (from email verification step)
    /// Returns SessionTokenData or null if invalid/expired
    /// </summary>
    public SessionTokenData? ValidateSessionToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for expiration
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Verify it's a session token
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "session")
            {
                _logger.LogWarning("Token validation failed: Not a session token");
                return null;
            }

            // Extract claims
            var emailClaim = principal.FindFirst("email");
            var isVerifiedClaim = principal.FindFirst("is_verified");

            if (emailClaim == null || isVerifiedClaim == null)
            {
                _logger.LogWarning("Token validation failed: Missing required claims");
                return null;
            }

            return new SessionTokenData
            {
                Email = emailClaim.Value,
                IsVerified = bool.Parse(isVerifiedClaim.Value)
            };
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Session token validation failed: Token expired");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Session token validation failed");
            return null;
        }
    }

    /// <summary>
    /// Validates access token
    /// Returns AccessTokenData or null if invalid/expired
    /// </summary>
    public AccessTokenData? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Verify it's an access token
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "access")
            {
                _logger.LogWarning("Token validation failed: Not an access token");
                return null;
            }

            // Extract claims
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var tenantIdClaim = principal.FindFirst("tenant_id");
            var emailClaim = principal.FindFirst("email");
            var usernameClaim = principal.FindFirst("username");
            var isAdminClaim = principal.FindFirst("is_admin");

            if (userIdClaim == null || tenantIdClaim == null || emailClaim == null ||
                usernameClaim == null || isAdminClaim == null)
            {
                _logger.LogWarning("Token validation failed: Missing required claims");
                return null;
            }

            return new AccessTokenData
            {
                UserId = int.Parse(userIdClaim.Value),
                TenantId = int.Parse(tenantIdClaim.Value),
                Email = emailClaim.Value,
                Username = usernameClaim.Value,
                IsAdmin = bool.Parse(isAdminClaim.Value)
            };
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Access token validation failed: Token expired");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Access token validation failed");
            return null;
        }
    }

    /// <summary>
    /// Validates refresh token from database
    /// Throws UnauthorizedException if invalid, expired, or revoked
    /// </summary>
    public async Task<RefreshToken> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // Find refresh token in database
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token validation failed: Token not found");
            throw new UnauthorizedException("Unauthorized");
        }

        // Check if token is expired
        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Refresh token validation failed: Token expired. UserId: {UserId}",
                refreshToken.UserId);

            throw new UnauthorizedException("Unauthorized");
        }

        // Check if token is revoked
        if (refreshToken.RevokedAt != null)
        {
            _logger.LogWarning(
                "Refresh token validation failed: Token revoked. UserId: {UserId}",
                refreshToken.UserId);

            throw new UnauthorizedException("Unauthorized");
        }

        // Check if user is active
        if (!refreshToken.User.IsActive)
        {
            _logger.LogWarning(
                "Refresh token validation failed: User inactive. UserId: {UserId}",
                refreshToken.UserId);

            throw new UnauthorizedException("Unauthorized");
        }

        _logger.LogInformation(
            "Refresh token validated successfully. UserId: {UserId}",
            refreshToken.UserId);

        return refreshToken;
    }
}
