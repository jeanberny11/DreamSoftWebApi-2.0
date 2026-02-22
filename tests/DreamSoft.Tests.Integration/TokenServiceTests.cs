using DreamSoft.Domain.Entities;
using DreamSoft.Infrastructure.Services.Common;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Tests for TokenService — verifies JWT generation and validation,
/// with particular focus on the purpose claim security guard.
/// </summary>
public class TokenServiceTests
{
    private readonly TokenService _sut;

    // Minimal in-memory configuration that satisfies TokenService
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "super-secret-key-that-is-long-enough-for-hmac256-validation",
                ["Jwt:Issuer"] = "dreamsoft-test",
                ["Jwt:Audience"] = "dreamsoft-test-audience",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
            })
            .Build();

    public TokenServiceTests()
    {
        _sut = new TokenService(BuildConfig());
    }

    // ---------------------------------------------------------------
    // GenerateRegistrationToken
    // ---------------------------------------------------------------

    [Fact]
    public void GenerateRegistrationToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRegistrationToken(tenantId: 42);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateRegistrationToken_IsValidJwt()
    {
        var token = _sut.GenerateRegistrationToken(tenantId: 42);

        // A valid JWT has exactly 3 parts separated by dots
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    // ---------------------------------------------------------------
    // GetTenantIdFromRegistrationToken — happy path
    // ---------------------------------------------------------------

    [Fact]
    public void GetTenantIdFromRegistrationToken_WithValidToken_ReturnsTenantId()
    {
        const int tenantId = 99;
        var token = _sut.GenerateRegistrationToken(tenantId);

        var result = _sut.GetTenantIdFromRegistrationToken(token);

        Assert.Equal(tenantId, result);
    }

    // ---------------------------------------------------------------
    // GetTenantIdFromRegistrationToken — security: must reject access tokens
    // ---------------------------------------------------------------

    [Fact]
    public void GetTenantIdFromRegistrationToken_WithAccessToken_ReturnsNull()
    {
        // An access token has no "purpose" = "registration" claim
        var user = User.Create(1, "admin@test.com", "admin@test.com", "hash", "Admin", "User");
        var tenant = Tenant.Create("ACME", "acme", "admin@test.com", 1, 1, 1);

        var accessToken = _sut.GenerateAccessToken(user, tenant);

        // Must be rejected — wrong purpose
        var result = _sut.GetTenantIdFromRegistrationToken(accessToken);

        Assert.Null(result);
    }

    [Fact]
    public void GetTenantIdFromRegistrationToken_WithInvalidString_ReturnsNull()
    {
        var result = _sut.GetTenantIdFromRegistrationToken("not.a.jwt");

        Assert.Null(result);
    }

    [Fact]
    public void GetTenantIdFromRegistrationToken_WithEmptyString_ReturnsNull()
    {
        var result = _sut.GetTenantIdFromRegistrationToken(string.Empty);

        Assert.Null(result);
    }

    [Fact]
    public void GetTenantIdFromRegistrationToken_WithTamperedToken_ReturnsNull()
    {
        var token = _sut.GenerateRegistrationToken(42);
        // Tamper with the signature
        var tampered = token[..^5] + "XXXXX";

        var result = _sut.GetTenantIdFromRegistrationToken(tampered);

        Assert.Null(result);
    }

    [Fact]
    public void GetTenantIdFromRegistrationToken_WithTokenFromDifferentSecret_ReturnsNull()
    {
        // Token signed with a different secret
        var otherConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "a-completely-different-secret-key-that-wont-match-at-all",
                ["Jwt:Issuer"] = "dreamsoft-test",
                ["Jwt:Audience"] = "dreamsoft-test-audience",
            })
            .Build();
        var otherService = new TokenService(otherConfig);
        var token = otherService.GenerateRegistrationToken(42);

        var result = _sut.GetTenantIdFromRegistrationToken(token);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // GenerateAccessToken
    // ---------------------------------------------------------------

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var user = User.Create(1, "user@test.com", "user@test.com", "hash", "Test", "User");
        var tenant = Tenant.Create("ACME Corp", "acme", "admin@acme.com", 1, 1, 1);

        var token = _sut.GenerateAccessToken(user, tenant);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateAccessToken_IsValidJwt()
    {
        var user = User.Create(1, "user@test.com", "user@test.com", "hash", "Test", "User");
        var tenant = Tenant.Create("ACME Corp", "acme", "admin@acme.com", 1, 1, 1);

        var token = _sut.GenerateAccessToken(user, tenant);

        Assert.Equal(3, token.Split('.').Length);
    }

    // ---------------------------------------------------------------
    // GenerateRefreshToken
    // ---------------------------------------------------------------

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateRefreshToken_EachCallReturnsUniqueValue()
    {
        var t1 = _sut.GenerateRefreshToken();
        var t2 = _sut.GenerateRefreshToken();

        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64Encoded()
    {
        var token = _sut.GenerateRefreshToken();

        // Should be decodeable as Base64
        var ex = Record.Exception(() => Convert.FromBase64String(token));
        Assert.Null(ex);
    }

    [Fact]
    public void GenerateRefreshToken_HasExpectedByteLength()
    {
        var token = _sut.GenerateRefreshToken();
        var bytes = Convert.FromBase64String(token);

        // GenerateRefreshToken uses RandomNumberGenerator.GetBytes(64)
        Assert.Equal(64, bytes.Length);
    }
}
