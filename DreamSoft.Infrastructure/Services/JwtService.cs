using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;

namespace DreamSoft.Infrastructure.Services;

public class JwtService : IJwtService
{
    public string GenerateAccessToken(int userId, int tenantId, string email, string username, bool isAdmin)
    {
        throw new NotImplementedException();
    }

    public string GenerateRefreshToken()
    {
        throw new NotImplementedException();
    }

    public string GenerateSessionToken(string email)
    {
        throw new NotImplementedException();
    }

    public AccessTokenData? ValidateAccessToken(string token)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshToken> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public SessionTokenData? ValidateSessionToken(string token)
    {
        throw new NotImplementedException();
    }
}