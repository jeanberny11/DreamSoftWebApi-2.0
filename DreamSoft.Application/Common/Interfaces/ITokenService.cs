using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Issues a short-lived JWT used ONLY to gate the verify-email step.
    /// Claims: sub = tenantId, purpose = "registration", exp = 24 h.
    /// This token cannot be used as an access token.
    /// </summary>
    string GenerateRegistrationToken(int tenantId);

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

    /// <summary>
    /// Validates a registration JWT and returns the tenantId from the sub claim.
    /// Returns null if the token is invalid, expired, or purpose != "registration".
    /// </summary>
    int? GetTenantIdFromRegistrationToken(string token);
}
