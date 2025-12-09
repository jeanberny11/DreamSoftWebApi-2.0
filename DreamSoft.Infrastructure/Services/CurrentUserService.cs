using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using DreamSoft.Application.Common.Interfaces;

namespace DreamSoft.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Username => throw new NotImplementedException();

    public string? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public int? LanguageId
    {
        get
        {
            var langIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("LanguageId")?.Value;
            if (int.TryParse(langIdClaim, out var langId))
            {
                return langId;
            }
            return null;
        }
    }

    public string? Email => throw new NotImplementedException();

    public bool IsAdmin => throw new NotImplementedException();

    public string? IpAddress => throw new NotImplementedException();

    int? ICurrentUserService.UserId => throw new NotImplementedException();

    int? ICurrentUserService.TenantId => throw new NotImplementedException();
}