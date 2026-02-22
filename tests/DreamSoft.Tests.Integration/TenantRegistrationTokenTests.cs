using DreamSoft.Domain.Entities;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Unit tests for TenantRegistrationToken domain entity methods.
/// No database required — pure domain logic tests.
/// </summary>
public class TenantRegistrationTokenTests
{
    private static readonly DateTime FixedUtc = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    // ---------------------------------------------------------------
    // Create factory
    // ---------------------------------------------------------------

    [Fact]
    public void Create_WithValidArgs_ReturnsToken()
    {
        var token = TenantRegistrationToken.Create(1, "hash123", FixedUtc.AddHours(24));

        Assert.Equal(1, token.TenantId);
        Assert.Equal("hash123", token.CodeHash);
        Assert.Equal(0, token.AttemptCount);
        Assert.False(token.IsConsumed);
    }

    [Fact]
    public void Create_WithZeroTenantId_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => TenantRegistrationToken.Create(0, "hash", FixedUtc.AddHours(1)));
        Assert.Contains("Tenant ID", ex.Message);
    }

    [Fact]
    public void Create_WithNegativeTenantId_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => TenantRegistrationToken.Create(-1, "hash", FixedUtc.AddHours(1)));
        Assert.Contains("Tenant ID", ex.Message);
    }

    [Fact]
    public void Create_WithEmptyCodeHash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => TenantRegistrationToken.Create(1, "", FixedUtc.AddHours(1)));
        Assert.Contains("Code hash", ex.Message);
    }

    [Fact]
    public void Create_WithWhitespaceCodeHash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => TenantRegistrationToken.Create(1, "   ", FixedUtc.AddHours(1)));
        Assert.Contains("Code hash", ex.Message);
    }

    // ---------------------------------------------------------------
    // IncrementAttempt
    // ---------------------------------------------------------------

    [Fact]
    public void IncrementAttempt_IncrementsAttemptCount()
    {
        var token = TenantRegistrationToken.Create(1, "hash", FixedUtc.AddHours(24));

        token.IncrementAttempt();
        token.IncrementAttempt();

        Assert.Equal(2, token.AttemptCount);
    }

    // ---------------------------------------------------------------
    // Consume
    // ---------------------------------------------------------------

    [Fact]
    public void Consume_SetsIsConsumedTrue()
    {
        var token = TenantRegistrationToken.Create(1, "hash", FixedUtc.AddHours(24));

        token.Consume();

        Assert.True(token.IsConsumed);
    }

    // ---------------------------------------------------------------
    // IsExpired
    // ---------------------------------------------------------------

    [Fact]
    public void IsExpired_WhenExpiresAtInFuture_ReturnsFalse()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddHours(24));

        Assert.False(token.IsExpired());
    }

    [Fact]
    public void IsExpired_WhenExpiresAtInPast_ReturnsTrue()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddSeconds(-1));

        Assert.True(token.IsExpired());
    }

    // ---------------------------------------------------------------
    // IsValid
    // ---------------------------------------------------------------

    [Fact]
    public void IsValid_WhenFreshToken_ReturnsTrue()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddHours(24));

        Assert.True(token.IsValid(maxAttempts: 5));
    }

    [Fact]
    public void IsValid_WhenConsumed_ReturnsFalse()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddHours(24));
        token.Consume();

        Assert.False(token.IsValid(maxAttempts: 5));
    }

    [Fact]
    public void IsValid_WhenExpired_ReturnsFalse()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddSeconds(-1));

        Assert.False(token.IsValid(maxAttempts: 5));
    }

    [Fact]
    public void IsValid_WhenAttemptCountEqualsMax_ReturnsFalse()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddHours(24));

        // Exhaust all allowed attempts
        for (var i = 0; i < 5; i++) token.IncrementAttempt();

        Assert.False(token.IsValid(maxAttempts: 5));
    }

    [Fact]
    public void IsValid_WhenAttemptCountBelowMax_ReturnsTrue()
    {
        var token = TenantRegistrationToken.Create(1, "hash",
            DateTime.UtcNow.AddHours(24));

        token.IncrementAttempt();
        token.IncrementAttempt();

        Assert.True(token.IsValid(maxAttempts: 5));
    }
}
