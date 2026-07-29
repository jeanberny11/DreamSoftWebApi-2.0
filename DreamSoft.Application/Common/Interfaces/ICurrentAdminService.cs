namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Provides identity context for requests authenticated with a superadmin token
/// (token_type = "superadmin", issued by AdminAuthController / SuperAdminScheme).
/// </summary>
/// <remarks>
/// SuperAdmin JWT claims:
/// <list type="table">
///   <item><term><c>sub</c> (ClaimTypes.NameIdentifier)</term><description>AdminUser.Id (int)</description></item>
///   <item><term><c>email</c></term><description>AdminUser.Email</description></item>
///   <item><term><c>full_name</c></term><description>AdminUser.GetFullName()</description></item>
///   <item><term><c>role_code</c></term><description>AdminUser.RoleCode</description></item>
///   <item><term><c>token_type</c></term><description>"superadmin"</description></item>
/// </list>
/// </remarks>
public interface ICurrentAdminService
{
    /// <summary>Gets the authenticated admin user's ID from the <c>sub</c> JWT claim.</summary>
    int? AdminId { get; }

    /// <summary>Gets the admin email from the JWT claims.</summary>
    string? Email { get; }

    /// <summary>Gets the admin's full name from the <c>full_name</c> JWT claim.</summary>
    string? FullName { get; }

    /// <summary>Gets the admin's role code from the <c>role_code</c> JWT claim.</summary>
    string? RoleCode { get; }

    /// <summary>Gets the IP address of the current request.</summary>
    string? IpAddress { get; }

    /// <summary>Checks if the request is authenticated.</summary>
    bool IsAuthenticated { get; }
}
