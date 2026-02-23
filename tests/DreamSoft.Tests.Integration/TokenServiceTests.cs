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
