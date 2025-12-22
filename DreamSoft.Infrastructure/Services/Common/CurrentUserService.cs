using System.Security.Claims;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DreamSoft.Infrastructure.Services.Common;

/// <summary>
/// Service for accessing current user context from HTTP request
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public int? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue("tenant_id");

            return int.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue("email");

    public string? Username => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue("username");

    public bool IsAdmin
    {
        get
        {
            var isAdminClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue("is_admin");

            return bool.TryParse(isAdminClaim, out var isAdmin) && isAdmin;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? Subdomain
    {
        get
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            
            if (string.IsNullOrEmpty(host))
                return null;

            // Extract subdomain from hostname
            // Examples:
            // - acme.dreamsoft.com → "acme"
            // - localhost → null
            // - dreamsoft.com → null
            
            var parts = host.Split('.');
            
            // If hostname has 3+ parts (subdomain.domain.tld), extract first part
            if (parts.Length >= 3)
            {
                return parts[0].ToLowerInvariant();
            }

            // No subdomain (localhost, dreamsoft.com, or IP address)
            return null;
        }
    }
}
