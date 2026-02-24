namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service to get current authenticated user information from HTTP context.
/// </summary>
/// <remarks>
/// <para><b>JWT Claim Schema</b> — every access token issued by the auth service MUST include:</para>
/// <list type="table">
///   <listheader><term>Claim name</term><description>Source / notes</description></listheader>
///   <item><term><c>sub</c> (ClaimTypes.NameIdentifier)</term><description>User.Id (int) — primary identity claim</description></item>
///   <item><term><c>tenant_id</c></term><description>Tenant.Id (int) — drives all tenant-scoped query filters</description></item>
///   <item><term><c>email</c></term><description>User.Email</description></item>
///   <item><term><c>username</c></term><description>User.Username</description></item>
///   <item><term><c>is_admin</c></term><description>"true" | "false" — system-level admin flag</description></item>
/// </list>
/// <para>
/// The <c>tenant_id</c> claim is the authoritative source of tenant context for authenticated requests.
/// For public (unauthenticated) endpoints the subdomain extracted from the <c>Host</c> header by
/// <c>TenantResolutionMiddleware</c> is the fallback, stored in <c>HttpContext.Items["Subdomain"]</c>.
/// </para>
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets current user ID from the <c>sub</c> JWT claim (<see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>).
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Gets current tenant ID from the <c>tenant_id</c> JWT claim.
    /// Null when the request is unauthenticated or the token was issued without a tenant context.
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Gets current user email from JWT token claims
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets current username from JWT token claims
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Checks if current user is admin
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the IP address of the current request
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the subdomain from the current HTTP request (e.g., acme.dreamsoft.com → "acme")
    /// </summary>
    string? Subdomain { get; }
}