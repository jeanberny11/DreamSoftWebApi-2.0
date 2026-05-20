using System.Security.Claims;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DreamSoft.Infrastructure.Services.Common;

public class CurrentAdminService(IHttpContextAccessor httpContextAccessor) : ICurrentAdminService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int? AdminId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email    => _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");
    public string? FullName => _httpContextAccessor.HttpContext?.User?.FindFirstValue("full_name");
    public string? RoleCode => _httpContextAccessor.HttpContext?.User?.FindFirstValue("role_code");

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
