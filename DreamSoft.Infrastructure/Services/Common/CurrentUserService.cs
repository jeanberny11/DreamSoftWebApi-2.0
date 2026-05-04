using System.Security.Claims;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DreamSoft.Infrastructure.Services.Common;

/// <summary>
/// Reads the current identity context from the JWT claims on the active HTTP request.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public int? TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public int? SolutionId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("solution_id");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email    => _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");
    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue("username");

    public bool IsAdmin
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("is_admin");
            return bool.TryParse(claim, out var isAdmin) && isAdmin;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public bool IsTenantIdentity =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("token_type") == "tenant";

    /// <inheritdoc />
    public bool IsUserIdentity =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("token_type") == "user";

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? Subdomain
    {
        get
        {
            var host  = _httpContextAccessor.HttpContext?.Request.Host.Host;
            if (string.IsNullOrEmpty(host)) return null;
            var parts = host.Split('.');
            return parts.Length >= 3 ? parts[0].ToLowerInvariant() : null;
        }
    }
}
