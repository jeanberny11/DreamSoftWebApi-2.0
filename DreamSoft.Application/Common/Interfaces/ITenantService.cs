namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Provides the current tenant and solution context for multi-tenant query filtering.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// The TenantId for the current request, or null when no tenant context exists
    /// (e.g. unauthenticated requests or system-level operations).
    /// </summary>
    int? CurrentTenantId { get; }

    /// <summary>
    /// The SolutionId for the current request, or null when no solution context exists
    /// (e.g. tenant account login, registration, or system-level operations).
    /// </summary>
    int? CurrentSolutionId { get; }
}
