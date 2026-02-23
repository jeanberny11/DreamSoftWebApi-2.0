using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Domain.Constants;
using DreamSoft.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Tests for RegisterTenantCommandHandler.
/// Uses SQLite in-memory database + stubs — no real infrastructure needed.
/// </summary>
public class RegisterTenantCommandHandlerTests : HandlerTestBase
{
    private readonly RegisterTenantCommandHandler _sut;

    public RegisterTenantCommandHandlerTests()
    {
        _sut = new RegisterTenantCommandHandler(
            Db, UnitOfWork, PasswordHasher, TokenService, EmailService, CurrentUser);
    }

    private static RegisterTenantCommand ValidCommand(
        string subdomain = "acme",
        string email = "admin@acme.com") =>
        new(
            CompanyName: "ACME Corp",
            Subdomain: subdomain,
            TaxId: null,
            Phone: "+18095551234",
            AddressLine1: "123 Main St",
            CountryId: 1,
            ProvinceId: 1,
            MunicipalityId: 1,
            AdminFirstName: "John",
            AdminLastName: "Doe",
            AdminEmail: email,
            AdminPassword: "Secret@123",
            LanguageId: 1,
            CurrencyId: 1
        );

    // ---------------------------------------------------------------
    // Happy path
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsRegistrationToken()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.RegistrationToken));
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesTenantInDb()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        var tenant = await Db.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "acme");
        Assert.NotNull(tenant);
        Assert.Equal("ACME Corp", tenant.CompanyName);
    }

    [Fact]
    public async Task Handle_WithValidCommand_TenantHasPendingEmailVerificationStatus()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        var tenant = await Db.Tenants
            .Include(t => t.Status)
            .FirstAsync(t => t.Subdomain == "acme");

        Assert.Equal(TenantStatusCodes.PendingEmailVerification, tenant.Status.Code);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesAdminUser()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        var user = await Db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "admin@acme.com");

        Assert.NotNull(user);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.False(user.IsEmailVerified);
    }

    [Fact]
    public async Task Handle_WithValidCommand_AdminPasswordIsHashed()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        var user = await Db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == "admin@acme.com");

        // StubPasswordHasher produces "hashed:{plain}"
        Assert.Equal("hashed:Secret@123", user.PasswordHash);
        Assert.NotEqual("Secret@123", user.PasswordHash);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesOtpToken()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        var tenant = await Db.Tenants.FirstAsync(t => t.Subdomain == "acme");
        var token = await Db.TenantRegistrationTokens
            .FirstOrDefaultAsync(t => t.TenantId == tenant.Id);

        Assert.NotNull(token);
        Assert.False(token.IsConsumed);
        Assert.Equal(0, token.AttemptCount);
    }

    [Fact]
    public async Task Handle_WithValidCommand_SendsVerificationEmail()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Single(EmailService.SentVerificationCodes);
        Assert.Equal("admin@acme.com", EmailService.SentVerificationCodes[0].To);
    }

    [Fact]
    public async Task Handle_WithValidCommand_RegistrationTokenContainsTenantId()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        var tenant = await Db.Tenants.FirstAsync(t => t.Subdomain == "acme");
        // StubTokenService returns "reg-token-{tenantId}"
        Assert.Equal($"reg-token-{tenant.Id}", result.RegistrationToken);
    }

    // ---------------------------------------------------------------
    // Subdomain conflict
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithDuplicateSubdomain_ThrowsConflictException()
    {
        await _sut.Handle(ValidCommand(subdomain: "taken"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.Handle(ValidCommand(subdomain: "taken", email: "other@test.com"), CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Email conflict
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithDuplicateEmail_ThrowsConflictException()
    {
        await _sut.Handle(ValidCommand(email: "dup@test.com"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.Handle(ValidCommand(subdomain: "other", email: "dup@test.com"), CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Missing seed data guard
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenPendingStatusMissing_ThrowsNotFoundException()
    {
        // Remove the seeded status
        await Db.Database.ExecuteSqlRawAsync(
            "DELETE FROM TenantStatuses WHERE Code = 'PENDING_EMAIL_VERIFICATION'");

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
