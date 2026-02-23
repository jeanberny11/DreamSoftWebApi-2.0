using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Onboarding.CompleteOnboarding;
using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Application.Features.Registration.VerifyEmail;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Tests for CompleteOnboardingCommandHandler.
/// Exercises solution/plan validation and tenant status transitions.
/// </summary>
public class CompleteOnboardingCommandHandlerTests : HandlerTestBase
{
    private readonly CompleteOnboardingCommandHandler _sut;
    private readonly RegisterTenantCommandHandler _registerHandler;
    private readonly VerifyEmailCommandHandler _verifyHandler;

    private int _seededSolutionId;
    private int _seededPlanId;
    private int _seededPlanWithTrialId;
    private int _seededOtherSolutionPlanId;

    public CompleteOnboardingCommandHandlerTests()
    {
        _registerHandler = new RegisterTenantCommandHandler(
            Db, UnitOfWork, PasswordHasher, TokenService, EmailService, CurrentUser);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimit:MaxVerificationAttemptsPerCode"] = "5",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
            })
            .Build();

        _verifyHandler = new VerifyEmailCommandHandler(
            Db, UnitOfWork, PasswordHasher, TokenService, EmailService, CurrentUser, config);

        _sut = new CompleteOnboardingCommandHandler(Db, UnitOfWork, CurrentUser);

        SeedSolutionsAndPlans();
    }

    private void SeedSolutionsAndPlans()
    {
        // Seed a billing cycle
        Db.Database.ExecuteSqlRaw(
            "INSERT INTO BillingCycles (Code, Name, Description, Months, IsActive, CreatedAt) VALUES ('MONTHLY', 'Monthly', 'Monthly billing', 1, 1, {0})",
            DateTime.UtcNow);
        var billingCycleId = (int)(long)Db.Database
            .ExecuteSqlRaw("SELECT last_insert_rowid()");
        // SQLite doesn't return values easily; query it back
        billingCycleId = Db.BillingCycles.First().Id;

        // Seed two solutions
        Db.Database.ExecuteSqlRaw(
            "INSERT INTO Solutions (Code, Name, Description, Icon, SortOrder, IsActive, CreatedAt) VALUES ('POS', 'POS Solution', 'Point of sale', 'pos-icon', 1, 1, {0})",
            DateTime.UtcNow);
        _seededSolutionId = Db.Solutions.First(s => s.Name == "POS Solution").Id;

        Db.Database.ExecuteSqlRaw(
            "INSERT INTO Solutions (Code, Name, Description, Icon, SortOrder, IsActive, CreatedAt) VALUES ('REST', 'Restaurant Solution', 'Restaurant management', 'rest-icon', 2, 1, {0})",
            DateTime.UtcNow);
        var otherSolutionId = Db.Solutions.First(s => s.Name == "Restaurant Solution").Id;

        // Seed plans
        Db.Database.ExecuteSqlRaw(
            "INSERT INTO SubscriptionPlans (Code, Name, Description, SolutionId, BillingCycleId, Price, TrialDays, IsActive, CreatedAt) VALUES ('BASIC', 'Basic Plan', 'Basic plan', {0}, {1}, 29.99, 0, 1, {2})",
            _seededSolutionId, billingCycleId, DateTime.UtcNow);
        _seededPlanId = Db.SubscriptionPlans.First(p => p.Code == "BASIC").Id;

        Db.Database.ExecuteSqlRaw(
            "INSERT INTO SubscriptionPlans (Code, Name, Description, SolutionId, BillingCycleId, Price, TrialDays, IsActive, CreatedAt) VALUES ('TRIAL', 'Trial Plan', 'Trial plan', {0}, {1}, 0, 14, 1, {2})",
            _seededSolutionId, billingCycleId, DateTime.UtcNow);
        _seededPlanWithTrialId = Db.SubscriptionPlans.First(p => p.Code == "TRIAL").Id;

        Db.Database.ExecuteSqlRaw(
            "INSERT INTO SubscriptionPlans (Code, Name, Description, SolutionId, BillingCycleId, Price, TrialDays, IsActive, CreatedAt) VALUES ('REST_BASIC', 'Restaurant Basic', 'Restaurant basic plan', {0}, {1}, 49.99, 0, 1, {2})",
            otherSolutionId, billingCycleId, DateTime.UtcNow);
        _seededOtherSolutionPlanId = Db.SubscriptionPlans.First(p => p.Code == "REST_BASIC").Id;
    }

    /// <summary>
    /// Registers + verifies a tenant, sets CurrentUser context,
    /// and returns the tenantId ready for onboarding.
    /// </summary>
    private async Task<int> RegisterAndVerifyAsync(
        string subdomain = "acme", string email = "admin@acme.com")
    {
        var cmd = new RegisterTenantCommand(
            "ACME Corp", subdomain, null, "+18095551234", "123 Main St",
            1, 1, 1, "John", "Doe", email, "Secret@123", 1, 1);

        var regResult = await _registerHandler.Handle(cmd, CancellationToken.None);
        var plainCode = EmailService.SentVerificationCodes.Last().Code;

        await _verifyHandler.Handle(
            new VerifyEmailCommand(regResult.RegistrationToken, plainCode),
            CancellationToken.None);

        var tenantId = await Db.Tenants
            .Where(t => t.Subdomain == subdomain)
            .Select(t => t.Id)
            .FirstAsync();

        var userId = await Db.Users.IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId)
            .Select(u => u.Id)
            .FirstAsync();

        CurrentUser.TenantId = tenantId;
        CurrentUser.UserId = userId;

        return tenantId;
    }

    // ---------------------------------------------------------------
    // Happy path — no trial
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithValidPlan_ReturnsRedirectUrl()
    {
        await RegisterAndVerifyAsync();

        var result = await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
            CancellationToken.None);

        Assert.Equal("/dashboard", result.RedirectUrl);
    }

    [Fact]
    public async Task Handle_WithValidPlan_TransitionsTenantToActive()
    {
        var tenantId = await RegisterAndVerifyAsync();

        await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
            CancellationToken.None);

        var tenant = await Db.Tenants
            .Include(t => t.Status)
            .FirstAsync(t => t.Id == tenantId);
        Assert.Equal(TenantStatusCodes.Active, tenant.Status.Code);
    }

    [Fact]
    public async Task Handle_WithValidPlan_CreatesTenantSubscription()
    {
        var tenantId = await RegisterAndVerifyAsync();

        await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
            CancellationToken.None);

        var subscription = await Db.TenantSubscriptions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);
        Assert.NotNull(subscription);
        Assert.Equal(_seededPlanId, subscription.SubscriptionPlanId);
    }

    [Fact]
    public async Task Handle_WithValidPlan_SubscriptionStatusIsActive()
    {
        var tenantId = await RegisterAndVerifyAsync();

        await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
            CancellationToken.None);

        var subscription = await Db.TenantSubscriptions
            .FirstAsync(s => s.TenantId == tenantId);
        var status = await Db.SubscriptionStatuses.FindAsync(subscription.StatusId);
        Assert.Equal(SubscriptionStatusCodes.Active, status!.Code);
    }

    // ---------------------------------------------------------------
    // Trial plan
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithTrialPlan_SubscriptionStatusIsTrial()
    {
        var tenantId = await RegisterAndVerifyAsync();

        await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanWithTrialId),
            CancellationToken.None);

        var subscription = await Db.TenantSubscriptions
            .FirstAsync(s => s.TenantId == tenantId);
        var status = await Db.SubscriptionStatuses.FindAsync(subscription.StatusId);
        Assert.Equal(SubscriptionStatusCodes.Trial, status!.Code);
    }

    [Fact]
    public async Task Handle_WithTrialPlan_SetsTrialEndDate()
    {
        var tenantId = await RegisterAndVerifyAsync();

        await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanWithTrialId),
            CancellationToken.None);

        var subscription = await Db.TenantSubscriptions
            .FirstAsync(s => s.TenantId == tenantId);
        Assert.NotNull(subscription.TrialEndDate);
        Assert.True(subscription.TrialEndDate > DateTime.UtcNow);
    }

    // ---------------------------------------------------------------
    // Plan does not belong to solution
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenPlanBelongsToDifferentSolution_ThrowsConflictException()
    {
        await RegisterAndVerifyAsync();

        // _seededOtherSolutionPlanId belongs to "Restaurant Solution", not "POS Solution"
        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.Handle(
                new CompleteOnboardingCommand(_seededSolutionId, _seededOtherSolutionPlanId),
                CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Solution not found
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithNonExistentSolution_ThrowsNotFoundException()
    {
        await RegisterAndVerifyAsync();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.Handle(
                new CompleteOnboardingCommand(9999, _seededPlanId),
                CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Plan not found
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WithNonExistentPlan_ThrowsNotFoundException()
    {
        await RegisterAndVerifyAsync();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.Handle(
                new CompleteOnboardingCommand(_seededSolutionId, 9999),
                CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Idempotency — already onboarded
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenAlreadyOnboarded_ThrowsConflictException()
    {
        await RegisterAndVerifyAsync();

        await _sut.Handle(
            new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
            CancellationToken.None);

        // Second attempt must be rejected
        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.Handle(
                new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
                CancellationToken.None));
    }

    // ---------------------------------------------------------------
    // Unauthenticated
    // ---------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenNoTenantIdInContext_ThrowsUnauthorizedException()
    {
        CurrentUser.TenantId = null;

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.Handle(
                new CompleteOnboardingCommand(_seededSolutionId, _seededPlanId),
                CancellationToken.None));
    }
}
