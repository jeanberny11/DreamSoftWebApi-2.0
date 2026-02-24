using DreamSoft.Application.Common.Interfaces;

namespace DreamSoft.Infrastructure.Services.Common;

/// <summary>
/// Resolves the current tenant ID from the authenticated user's JWT claims.
/// </summary>
public class TenantService(ICurrentUserService currentUserService) : ITenantService
{
    public int? CurrentTenantId => currentUserService.TenantId;
}
