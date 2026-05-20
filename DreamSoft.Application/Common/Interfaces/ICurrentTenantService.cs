namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Provides identity context for requests authenticated with a tenant token
/// (token_type = "tenant", issued by TenantAuthController / TenantScheme).
/// </summary>
/// <remarks>
/// Tenant JWT claims:
/// <list type="table">
///   <item><term><c>sub</c> / <c>tenant_id</c></term><description>Tenant.Id (int)</description></item>
///   <item><term><c>email</c></term><description>Tenant.Email</description></item>
///   <item><term><c>token_type</c></term><description>"tenant"</description></item>
/// </list>
/// </remarks>
public interface ICurrentTenantService
{
    /// <summary>Gets the authenticated tenant's ID from the <c>tenant_id</c> JWT claim.</summary>
    int? TenantId { get; }

    /// <summary>Gets the IP address of the current request.</summary>
    string? IpAddress { get; }

    /// <summary>Checks if the request is authenticated.</summary>
    bool IsAuthenticated { get; }
}
