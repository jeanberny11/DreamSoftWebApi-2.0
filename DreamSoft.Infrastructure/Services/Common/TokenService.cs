using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DreamSoft.Infrastructure.Services.Common;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration _config = configuration;

    public string GenerateAccessToken(User user, Tenant tenant)
    {
        var expiry = int.Parse(
            _config["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("tenant_id", tenant.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("username", user.Username),
            new Claim("is_admin", user.IsAdmin.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        return BuildToken(claims, TimeSpan.FromMinutes(expiry));
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    // ── Private helpers ────────────────────────────────────────
    private string BuildToken(IEnumerable<Claim> claims, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
