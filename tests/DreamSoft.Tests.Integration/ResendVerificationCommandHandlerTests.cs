using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Application.Features.Registration.ResendVerification;
using DreamSoft.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Tests for ResendVerificationCommandHandler.
/// Verifies rate limiting, token invalidation, and new OTP creation.
/// </summary>
public class ResendVerificationCommandHandlerTests : HandlerTestBase
{
    private readonly ResendVerificationCommandHandler _sut;
    private readonly RegisterTenantCommandHandler _registerHandler;

    public ResendVerificationCommandHandlerTests()
    {
        _registerHandler = new RegisterTenantCommandHandler(
            Db, UnitOfWork, PasswordHasher, TokenService, EmailService);

        _sut = new ResendVerificationCommandHandler(
            Db, PasswordHasher, TokenService, EmailService);
    }

    private async Task<(string RegToken, int TenantId)> RegisterAsync(
        string subdomain = "acme", string email = "admin@acme.com")
    {
        var cmd = new RegisterTenantCommand(
            "ACME Corp", subdomain, null, "+18095551234", "123 Main St",
            1, 1, 1, "John", "Doe", email, "Secret@123", 1, 1);

        var result = await _registerHandler.Handle(cmd, CancellationToken.None);
        var tenantId = int.Parse(result.RegistrationToken["reg-token-".Length..]);
        return (result.RegistrationToken, tenantId);
    }

    // ---------------------------------------------------------------
    // Happy path
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_AfterCooldown_ReturnsUnit()
    {
        var (regToken, tenantId) = await RegisterAsync();

        // Push the existing token's CreatedAt into the past (> 2 min ago)
        await Db.Database.ExecuteSqlRawAsync(
            "UPDATE TenantRegistrationTokens SET CreatedAt = datetime('now', '-3 minutes') WHERE TenantId = {0}",
            tenantId);

        var result = await _sut.Handle(
            new ResendVerificationCommand(regToken), CancellationToken.None);

        Assert.Equal(MediatR.Unit.Value, result);
    }

    [Fact]
    public async Task Handle_AfterCooldown_SendsNewVerificationEmail()
    {
        var (regToken, tenantId) = await RegisterAsync();
        var initialEmailCount = EmailService.SentVerificationCodes.Count;

        await Db.Database.ExecuteSqlRawAsync(
            "UPDATE TenantRegistrationTokens SET CreatedAt = datetime('now', '-3 minutes') WHERE TenantId = {0}",
            tenantId);

        await _sut.Handle(new ResendVerificationCommand(regToken), CancellationToken.None);

        Assert.Equal(initialEmailCount + 1, EmailService.SentVerificationCodes.Count);
    }

    [Fact]
    public async Task Handle_AfterCooldown_InvalidatesOldToken()
    {
        var (regToken, tenantId) = await RegisterAsync();

        await Db.Database.ExecuteSqlRawAsync(
            "UPDATE TenantRegistrationTokens SET CreatedAt = datetime('now', '-3 minutes') WHERE TenantId = {0}",
            tenantId);

        await _sut.Handle(new ResendVerificationCommand(regToken), CancellationToken.None);

        // All previous tokens must be consumed
        var unconsumed = await Db.TenantRegistrationTokens
            .Where(t => t.TenantId == tenantId && !t.IsConsumed)
            .ToListAsync();

        // Only the NEW token should remain unconsumed
        Assert.Single(unconsumed);
    }

    [Fact]
    public async Task Handle_AfterCooldown_CreatesNewOtpToken()
    {
        var (regToken, tenantId) = await RegisterAsync();
        var tokensBefore = await Db.TenantRegistrationTokens
            .Where(t => t.TenantId == tenantId).CountAsync();

        await Db.Database.ExecuteSqlRawAsync(
            "UPDATE TenantRegistrationTokens SET CreatedAt = datetime('now', '-3 minutes') WHERE TenantId = {0}",
            tenantId);

        await _sut.Handle(new ResendVerificationCommand(regToken), CancellationToken.None);

        var tokensAfter = await Db.TenantRegistrationTokens
            .Where(t => t.TenantId == tenantId).CountAsync();
        Assert.Equal(tokensBefore + 1, tokensAfter);
    }

    // ---------------------------------------------------------------
    // Rate limiting
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithinCooldownWindow_ThrowsRateLimitExceededException()
    {
        var (regToken, _) = await RegisterAsync();

        // No time manipulation — token was just created (within 2-minute window)
        await Assert.ThrowsAsync<RateLimitExceededException>(
            () => _sut.Handle(new ResendVerificationCommand(regToken), CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Invalid token
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithInvalidRegistrationToken_ThrowsUnauthorizedException()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.Handle(
                new ResendVerificationCommand("not-a-real-token"),
                CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Already verified (wrong status)
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenTenantAlreadyVerified_ThrowsConflictException()
    {
        var (regToken, tenantId) = await RegisterAsync();

        // Manually move tenant to PENDING_SUBSCRIPTION status
        await Db.Database.ExecuteSqlRawAsync(
            "UPDATE Tenants SET StatusId = (SELECT Id FROM TenantStatuses WHERE Code = 'PENDING_SUBSCRIPTION') WHERE Id = {0}",
            tenantId);

        await Db.Database.ExecuteSqlRawAsync(
            "UPDATE TenantRegistrationTokens SET CreatedAt = datetime('now', '-3 minutes') WHERE TenantId = {0}",
            tenantId);

        // Clear the change tracker so the handler re-fetches the updated tenant from the DB
        Db.ChangeTracker.Clear();

        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.Handle(new ResendVerificationCommand(regToken), CancellationToken.None));
    }
}
