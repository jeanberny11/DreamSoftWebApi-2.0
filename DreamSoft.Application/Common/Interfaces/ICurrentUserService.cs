namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service to get current authenticated user information from HTTP context.
/// </summary>
/// <remarks>
/// <para><b>JWT Claim Schema</b></para>
/// <para><b>User tokens</b> (AuthController / UserScheme):</para>
/// <list type="table">
///   <listheader><term>Claim</term><description>Notes</description></listheader>
///   <item><term><c>sub</c> (ClaimTypes.NameIdentifier)</term><description>User.Id (int)</description></item>
///   <item><term><c>tenant_id</c></term><description>Tenant.Id (int)</description></item>
///   <item><term><c>solution_id</c></term><description>Solution.Id (int)</description></item>
///   <item><term><c>email</c></term><description>User.Email</description></item>
///   <item><term><c>username</c></term><description>User.Username</description></item>
///   <item><term><c>is_admin</c></term><description>"true" | "false"</description></item>
///   <item><term><c>token_type</c></term><description>"user"</description></item>
/// </list>
/// <para><b>Tenant tokens</b> (TenantAuthController / TenantScheme):</para>
/// <list type="table">
///   <item><term><c>sub</c> (ClaimTypes.NameIdentifier)</term><description>Tenant.Id (int)</description></item>
///   <item><term><c>tenant_id</c></term><description>Tenant.Id (int)</description></item>
///   <item><term><c>email</c></term><description>Tenant.Email</description></item>
///   <item><term><c>token_type</c></term><description>"tenant"</description></item>
/// </list>
/// <para>
/// Use <see cref="IsTenantIdentity"/> and <see cref="IsUserIdentity"/> to branch logic
/// by identity type rather than checking claims directly.
/// </para>
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>Gets the subject ID from the <c>sub</c> claim (User.Id for users, Tenant.Id for tenants).</summary>
    int? UserId { get; }

    /// <summary>Gets current tenant ID from the <c>tenant_id</c> JWT claim. Present in both token types.</summary>
    int? TenantId { get; }

    /// <summary>Gets current solution ID from the <c>solution_id</c> claim. Only present in User tokens.</summary>
    int? SolutionId { get; }

    /// <summary>Gets the email from the JWT token claims.</summary>
    string? Email { get; }

    /// <summary>Gets the username from JWT token claims. Only present in User tokens.</summary>
    string? Username { get; }

    /// <summary>Checks if current user is an admin. Only meaningful for User tokens.</summary>
    bool IsAdmin { get; }

    /// <summary>Checks if the request is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns true when the current token was issued by TenantAuthController
    /// (token_type = "tenant"). Use to gate tenant account management endpoints.
    /// </summary>
    bool IsTenantIdentity { get; }

    /// <summary>
    /// Returns true when the current token was issued by AuthController
    /// (token_type = "user"). Use to gate solution feature endpoints.
    /// </summary>
    bool IsUserIdentity { get; }

    /// <summary>Gets the IP address of the current request.</summary>
    string? IpAddress { get; }

    /// <summary>Gets the subdomain from the current HTTP request host header.</summary>
    string? Subdomain { get; }
}
