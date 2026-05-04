using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Issues an access token for an authenticated solution User.
    /// Claims: sub, tenant_id, solution_id, email, username, is_admin.
    /// </summary>
    string GenerateAccessToken(User user, Tenant tenant);

    /// <summary>
    /// Issues an access token for an authenticated Tenant account login (no solution context).
    /// Claims: sub (tenantId), tenant_id, email.
    /// </summary>
    string GenerateTenantAccessToken(Tenant tenant);

    /// <summary>
    /// Issues an access token for an authenticated AdminUser.
    /// Claims: sub (adminUserId), email, role_code, token_type=superadmin.
    /// Signed with a dedicated secret (Jwt:SuperAdmin).
    /// </summary>
    string GenerateSuperAdminToken(AdminUser adminUser);

    /// <summary>
    /// Generates a cryptographically random opaque string for refresh tokens.
    /// </summary>
    string GenerateRefreshToken();
}
