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

    /// <summary>
    /// Access token for solution User login.
    /// Signed with the User secret. Includes solution_id, is_admin, and token_type=user.
    /// </summary>
    public string GenerateAccessToken(User user, Tenant tenant)
    {
        var expiry = int.Parse(_config["Jwt:User:AccessTokenExpirationMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("tenant_id",   tenant.Id.ToString()),
            new Claim("solution_id", user.SolutionId.ToString()),
            new Claim("email",       user.Email),
            new Claim("username",    user.Username),
            new Claim("is_admin",    user.IsAdmin.ToString().ToLower()),
            new Claim("token_type",  "user"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return BuildToken(claims, TimeSpan.FromMinutes(expiry), "Jwt:User");
    }

    /// <summary>
    /// Access token for Tenant account login.
    /// Signed with the Tenant secret. No solution_id. token_type=tenant.
    /// </summary>
    public string GenerateTenantAccessToken(Tenant tenant)
    {
        var expiry = int.Parse(_config["Jwt:Tenant:AccessTokenExpirationMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, tenant.Id.ToString()),
            new Claim("tenant_id",  tenant.Id.ToString()),
            new Claim("email",      tenant.Email),
            new Claim("token_type", "tenant"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return BuildToken(claims, TimeSpan.FromMinutes(expiry), "Jwt:Tenant");
    }

    /// <summary>
    /// Access token for AdminUser (SuperAdmin) login.
    /// Signed with a dedicated secret (Jwt:SuperAdmin). token_type=superadmin.
    /// </summary>
    public string GenerateSuperAdminToken(AdminUser adminUser)
    {
        var expiry = int.Parse(_config["Jwt:SuperAdmin:AccessTokenExpirationMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new Claim("email",      adminUser.Email),
            new Claim("full_name",  adminUser.GetFullName()),
            new Claim("role_code",  adminUser.RoleCode),
            new Claim("token_type", "superadmin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return BuildToken(claims, TimeSpan.FromMinutes(expiry), "Jwt:SuperAdmin");
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds and signs a JWT using the secret and settings at the given config section path.
    /// </summary>
    private string BuildToken(IEnumerable<Claim> claims, TimeSpan expiry, string configSection)
    {
        var secret  = _config[$"{configSection}:Secret"]
                      ?? throw new InvalidOperationException($"JWT Secret not configured at {configSection}:Secret");
        var issuer   = _config[$"{configSection}:Issuer"];
        var audience = _config[$"{configSection}:Audience"];

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.Add(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
