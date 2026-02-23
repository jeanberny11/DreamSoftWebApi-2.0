using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Issues the real access token for authenticated API calls.
    /// Claims: sub = userId, tenant_id, email, username, is_admin.
    /// Expiry: Jwt:AccessTokenExpirationMinutes from configuration.
    /// </summary>
    string GenerateAccessToken(User user, Tenant tenant);

    /// <summary>
    /// Generates a cryptographically random opaque string for refresh tokens.
    /// </summary>
    string GenerateRefreshToken();
}
