using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Application.Features.Registration.VerifyEmail;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Tests for VerifyEmailCommandHandler.
/// Exercises OTP validation, attempt counting, status transitions,
/// and access token issuance.
/// </summary>
public class VerifyEmailCommandHandlerTests : HandlerTestBase
{
    private readonly VerifyEmailCommandHandler _sut;
    private readonly RegisterTenantCommandHandler _registerHandler;
    private readonly IConfiguration _config;

    public VerifyEmailCommandHandlerTests()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimit:MaxVerificationAttemptsPerCode"] = "5",
                ["RateLimit:MaxVerifyEmailAttemptsPerWindow"] = "5",
                ["RateLimit:VerifyEmailWindowMinutes"] = "10",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
            })
            .Build();

        _registerHandler = new RegisterTenantCommandHandler(
            Db, UnitOfWork, PasswordHasher, EmailService, CurrentUser);

        _sut = new VerifyEmailCommandHandler(
            Db, UnitOfWork, PasswordHasher, TokenService, EmailService, CurrentUser, RateLimitService, _config);
    }

    /// <summary>
    /// Registers a tenant and returns the plain OTP code and tenantId.
    /// </summary>
    private async Task<(string PlainCode, int TenantId)> RegisterAsync(
        string subdomain = "acme", string email = "admin@acme.com")
    {
        var cmd = new RegisterTenantCommand(
            "ACME Corp", subdomain, null, "+18095551234", "123 Main St",
            1, 1, 1, "John", "Doe", email, "Secret@123", 1, 1);

        var result = await _registerHandler.Handle(cmd, CancellationToken.None);

        // StubEmailService captures the plain code
        var sentCode = EmailService.SentVerificationCodes.Last().Code;

        // Lookup tenant id from the email returned in response
        var tenant = await Db.Tenants.FirstAsync(t => t.Email == result.Email);

        return (sentCode, tenant.Id);
    }

    // ---------------------------------------------------------------
    // Happy path
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithCorrectCode_ReturnsAccessAndRefreshTokens()
    {
        var (plainCode, _) = await RegisterAsync();

        var result = await _sut.Handle(
            new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task Handle_WithCorrectCode_TransitionsTenantToPendingSubscription()
    {
        var (plainCode, tenantId) = await RegisterAsync();

        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        var tenant = await Db.Tenants
            .Include(t => t.Status)
            .FirstAsync(t => t.Id == tenantId);
        Assert.Equal(TenantStatusCodes.PendingSubscription, tenant.Status.Code);
    }

    [Fact]
    public async Task Handle_WithCorrectCode_VerifiesTenantEmail()
    {
        var (plainCode, tenantId) = await RegisterAsync();

        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        var tenant = await Db.Tenants.FindAsync(tenantId);
        Assert.True(tenant!.EmailVerified);
    }

    [Fact]
    public async Task Handle_WithCorrectCode_VerifiesAdminUserEmail()
    {
        var (plainCode, tenantId) = await RegisterAsync();

        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        var user = await Db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.TenantId == tenantId);
        Assert.True(user.IsEmailVerified);
    }

    [Fact]
    public async Task Handle_WithCorrectCode_ConsumesOtpToken()
    {
        var (plainCode, tenantId) = await RegisterAsync();

        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        var token = await Db.TenantRegistrationTokens
            .FirstAsync(t => t.TenantId == tenantId);
        Assert.True(token.IsConsumed);
    }

    [Fact]
    public async Task Handle_WithCorrectCode_SendsWelcomeEmail()
    {
        var (plainCode, _) = await RegisterAsync();

        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        Assert.Single(EmailService.SentWelcomeEmails);
    }

    [Fact]
    public async Task Handle_WithCorrectCode_PersistsRefreshToken()
    {
        var (plainCode, tenantId) = await RegisterAsync();

        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        var user = await Db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.TenantId == tenantId);

        // Refresh token is now stored in the refresh_tokens table, not on the User entity
        var tokenExists = await Db.RefreshTokens
            .AnyAsync(rt => rt.UserId == user.Id);
        Assert.True(tokenExists);
    }

    // ---------------------------------------------------------------
    // Wrong code — increments attempt count
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithWrongCode_ThrowsUnauthorizedException()
    {
        await RegisterAsync();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.Handle(new VerifyEmailCommand("admin@acme.com", "000000"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithWrongCode_IncrementsAttemptCount()
    {
        var (_, tenantId) = await RegisterAsync();

        try { await _sut.Handle(new VerifyEmailCommand("admin@acme.com", "000000"), CancellationToken.None); }
        catch (UnauthorizedException) { }

        var token = await Db.TenantRegistrationTokens
            .FirstAsync(t => t.TenantId == tenantId);
        Assert.Equal(1, token.AttemptCount);
    }

    [Fact]
    public async Task Handle_AfterMaxAttempts_ThrowsUnauthorizedException()
    {
        await RegisterAsync();

        // Exhaust all 5 attempts with wrong codes
        for (var i = 0; i < 5; i++)
        {
            try { await _sut.Handle(new VerifyEmailCommand("admin@acme.com", "000000"), CancellationToken.None); }
            catch (UnauthorizedException) { }
        }

        // 6th attempt — even with correct code — must fail (token invalid)
        var correctCode = EmailService.SentVerificationCodes.Last().Code;
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.Handle(new VerifyEmailCommand("admin@acme.com", correctCode), CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Invalid / tampered registration token
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsUnauthorizedException()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.Handle(
                new VerifyEmailCommand("nobody@unknown.com", "123456"),
                CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Idempotency — already verified
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenAlreadyVerified_ThrowsConflictException()
    {
        var (plainCode, _) = await RegisterAsync();

        // First verify succeeds
        await _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None);

        // Second attempt must be rejected
        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.Handle(new VerifyEmailCommand("admin@acme.com", plainCode), CancellationToken.None));
    }
}
