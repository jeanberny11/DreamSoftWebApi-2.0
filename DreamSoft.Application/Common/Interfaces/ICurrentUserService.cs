namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service to get current authenticated user information from HTTP context
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets current user ID from JWT token claims
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Gets current tenant ID from JWT token claims
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
}