using System.Security.Claims;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DreamSoft.Infrastructure.Services.Common;

public class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int? TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
