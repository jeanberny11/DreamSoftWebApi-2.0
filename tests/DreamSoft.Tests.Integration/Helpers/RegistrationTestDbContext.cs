using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Common;
using DreamSoft.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Tests.Integration.Helpers;

// ---------------------------------------------------------------
// Stubs shared across all registration/onboarding handler tests
// ---------------------------------------------------------------

public sealed class StubPasswordHasher : IPasswordHasher
{
    // Identity hasher — hash == plaintext, for deterministic tests
    public string HashPassword(string password) => $"hashed:{password}";
    public bool VerifyPassword(string password, string hash) => hash == $"hashed:{password}";
}

public sealed class StubTokenService : ITokenService
{
    public string GenerateAccessToken(Domain.Entities.User user, Domain.Entities.Tenant tenant) => $"access-token-{user.Id}";
    public string GenerateRefreshToken() => "refresh-token-fixed";
}

public sealed class StubEmailService : IEmailService
{
    public List<(string To, string Code)> SentVerificationCodes { get; } = [];
    public List<string> SentWelcomeEmails { get; } = [];

    public Task<bool> SendVerificationCodeAsync(string toEmail, string code, CancellationToken ct = default)
    {
        SentVerificationCodes.Add((toEmail, code));
        return Task.FromResult(true);
    }
    public Task SendWelcomeEmailAsync(string toEmail, string firstName, string companyName, string subdomain, CancellationToken ct = default)
    {
        SentWelcomeEmails.Add(toEmail);
        return Task.CompletedTask;
    }
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
        => Task.CompletedTask;
}

public sealed class StubCurrentUserService : ICurrentUserService
{
    public int? UserId { get; set; }
    public int? TenantId { get; set; }
    public string? Email => null;
    public string? Username => null;
    public bool IsAdmin => false;
    public bool IsAuthenticated => UserId.HasValue;
    public string? IpAddress => null;
    public string? Subdomain => null;
}

public sealed class StubDateTime : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class StubTenantService : ITenantService
{
    public int? CurrentTenantId { get; set; }
}

public sealed class StubRateLimitService : IRateLimitService
{
    /// <summary>Always allow — rate limiting is exercised at the unit/integration level separately.</summary>
    public bool IsAllowed(string key, int maxAttempts, int windowMinutes) => true;
}

// ---------------------------------------------------------------
// Minimal in-memory DbContext for handler tests
// Maps only the tables needed: Tenants, TenantStatuses,
// SubscriptionStatuses, Users, TenantRegistrationTokens,
// Solutions, SubscriptionPlans, TenantSubscriptions.
// Uses SQLite (no Npgsql-specific features).
// ---------------------------------------------------------------
public sealed class RegistrationTestDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantService _tenantService;
    private readonly IDateTime _dateTime;

    public RegistrationTestDbContext(
        DbContextOptions<RegistrationTestDbContext> options,
        ICurrentUserService currentUserService,
        ITenantService tenantService,
        IDateTime dateTime)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantService = tenantService;
        _dateTime = dateTime;
    }

    // IApplicationDbContext DbSets (lookup)
    public DbSet<Domain.Entities.BillingCycle> BillingCycles => Set<Domain.Entities.BillingCycle>();
    public DbSet<Domain.Entities.Country> Countries => Set<Domain.Entities.Country>();
    public DbSet<Domain.Entities.Currency> Currencies => Set<Domain.Entities.Currency>();
    public DbSet<Domain.Entities.Gender> Genders => Set<Domain.Entities.Gender>();
    public DbSet<Domain.Entities.IdType> IdTypes => Set<Domain.Entities.IdType>();
    public DbSet<Domain.Entities.Language> Languages => Set<Domain.Entities.Language>();
    public DbSet<Domain.Entities.MenuGroup> MenuGroups => Set<Domain.Entities.MenuGroup>();
    public DbSet<Domain.Entities.MenuOption> MenuOptions => Set<Domain.Entities.MenuOption>();
    public DbSet<Domain.Entities.Module> Modules => Set<Domain.Entities.Module>();
    public DbSet<Domain.Entities.Municipality> Municipalities => Set<Domain.Entities.Municipality>();
    public DbSet<Domain.Entities.OptionAction> OptionActions => Set<Domain.Entities.OptionAction>();
    public DbSet<Domain.Entities.Province> Provinces => Set<Domain.Entities.Province>();
    public DbSet<Domain.Entities.Solution> Solutions => Set<Domain.Entities.Solution>();
    public DbSet<Domain.Entities.SubscriptionStatus> SubscriptionStatuses => Set<Domain.Entities.SubscriptionStatus>();
    public DbSet<Domain.Entities.TenantStatus> TenantStatuses => Set<Domain.Entities.TenantStatus>();

    // IApplicationDbContext DbSets (business)
    public DbSet<Domain.Entities.RoleTemplate> RoleTemplates => Set<Domain.Entities.RoleTemplate>();
    public DbSet<Domain.Entities.SubscriptionPlan> SubscriptionPlans => Set<Domain.Entities.SubscriptionPlan>();
    public DbSet<Domain.Entities.Tenant> Tenants => Set<Domain.Entities.Tenant>();
    public DbSet<Domain.Entities.TenantSubscription> TenantSubscriptions => Set<Domain.Entities.TenantSubscription>();
    public DbSet<Domain.Entities.TenantRegistrationToken> TenantRegistrationTokens => Set<Domain.Entities.TenantRegistrationToken>();
    public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
    public DbSet<Domain.Entities.Role> Roles => Set<Domain.Entities.Role>();
    public DbSet<Domain.Entities.RefreshToken> RefreshTokens => Set<Domain.Entities.RefreshToken>();

    // Junction tables (not exercised in these tests but required by interface)
    public DbSet<Domain.Entities.SolutionMenuOption> SolutionMenuOptions => Set<Domain.Entities.SolutionMenuOption>();
    public DbSet<Domain.Entities.RoleMenuOption> RoleMenuOptions => Set<Domain.Entities.RoleMenuOption>();
    public DbSet<Domain.Entities.RoleMenuOptionTemplate> RoleMenuOptionTemplates => Set<Domain.Entities.RoleMenuOptionTemplate>();
    public DbSet<Domain.Entities.RoleOptionAction> RoleOptionActions => Set<Domain.Entities.RoleOptionAction>();
    public DbSet<Domain.Entities.RoleOptionActionTemplate> RoleOptionActionTemplates => Set<Domain.Entities.RoleOptionActionTemplate>();
    public DbSet<Domain.Entities.UserRole> UserRoles => Set<Domain.Entities.UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TenantStatus
        modelBuilder.Entity<Domain.Entities.TenantStatus>(b =>
        {
            b.ToTable("TenantStatuses");
            b.HasKey(e => e.Id);
            b.Property(e => e.Code).IsRequired().HasMaxLength(100);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Ignore(e => e.Translations);
            b.Ignore(e => e.Tenants);
        });

        // SubscriptionStatus
        modelBuilder.Entity<Domain.Entities.SubscriptionStatus>(b =>
        {
            b.ToTable("SubscriptionStatuses");
            b.HasKey(e => e.Id);
            b.Property(e => e.Code).IsRequired().HasMaxLength(100);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Ignore(e => e.Translations);
            b.Ignore(e => e.TenantSubscriptions);
        });

        // Tenant
        modelBuilder.Entity<Domain.Entities.Tenant>(b =>
        {
            b.ToTable("Tenants");
            b.HasKey(e => e.Id);
            b.Property(e => e.CompanyName).IsRequired().HasMaxLength(255);
            b.Property(e => e.Subdomain).IsRequired().HasMaxLength(50);
            b.Property(e => e.Email).IsRequired().HasMaxLength(255);
            b.Property(e => e.Phone).HasMaxLength(50);
            b.Property(e => e.TaxId).HasMaxLength(50);
            b.Property(e => e.Website).HasMaxLength(255);
            b.Property(e => e.AddressLine1).HasMaxLength(255);
            b.Property(e => e.AddressLine2).HasMaxLength(255);
            b.Property(e => e.PostalCode).HasMaxLength(20);
            b.Property(e => e.LogoUrl).HasMaxLength(500);
            b.Ignore(e => e.Country);
            b.Ignore(e => e.Province);
            b.Ignore(e => e.Municipality);
            b.Ignore(e => e.Language);
            b.Ignore(e => e.Currency);
            b.Ignore(e => e.Users);
            b.Ignore(e => e.Roles);
            b.Ignore(e => e.TenantSubscriptions);
            b.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User
        modelBuilder.Entity<Domain.Entities.User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(e => e.Id);
            b.Property(e => e.TenantId).IsRequired();
            b.Property(e => e.Username).IsRequired().HasMaxLength(100);
            b.Property(e => e.Email).IsRequired().HasMaxLength(255);
            b.Property(e => e.PasswordHash).IsRequired();
            b.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            b.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            b.Ignore(e => e.Gender);
            b.Ignore(e => e.IdType);
            b.Ignore(e => e.Language);
            b.Ignore(e => e.UserRoles);
            b.Ignore(e => e.AssignedUserRoles);
            b.Ignore(e => e.CreatedUsers);
            b.Ignore(e => e.UpdatedUsers);
            b.Ignore(e => e.CreatedRoles);
            b.Ignore(e => e.UpdatedRoles);
            b.Ignore(e => e.Tenant);
            b.Ignore(e => e.CreatedByUser);
            b.Ignore(e => e.UpdatedByUser);
            b.Ignore(e => e.RefreshTokens);
            b.HasQueryFilter(e => _tenantService.CurrentTenantId == null
                               || e.TenantId == _tenantService.CurrentTenantId);
        });

        // TenantRegistrationToken
        modelBuilder.Entity<Domain.Entities.TenantRegistrationToken>(b =>
        {
            b.ToTable("TenantRegistrationTokens");
            b.HasKey(e => e.Id);
            b.Property(e => e.TenantId).IsRequired();
            b.Property(e => e.CodeHash).IsRequired().HasMaxLength(512);
            b.Property(e => e.ExpiresAt).IsRequired();
            b.Property(e => e.AttemptCount).HasDefaultValue(0).IsRequired();
            b.Property(e => e.IsConsumed).HasDefaultValue(false).IsRequired();
            b.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Solution
        modelBuilder.Entity<Domain.Entities.Solution>(b =>
        {
            b.ToTable("Solutions");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Ignore(e => e.Translations);
            b.Ignore(e => e.SolutionMenuOptions);
            b.Ignore(e => e.SubscriptionPlans);
        });

        // BillingCycle
        modelBuilder.Entity<Domain.Entities.BillingCycle>(b =>
        {
            b.ToTable("BillingCycles");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Ignore(e => e.Translations);
            b.Ignore(e => e.SubscriptionPlans);
        });

        // SubscriptionPlan
        modelBuilder.Entity<Domain.Entities.SubscriptionPlan>(b =>
        {
            b.ToTable("SubscriptionPlans");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Code).IsRequired().HasMaxLength(50);
            b.Property(e => e.Price).HasColumnType("decimal(18,2)");
            b.Ignore(e => e.Translations);
            b.Ignore(e => e.TenantSubscriptions);
            b.HasOne(e => e.Solution)
                .WithMany()
                .HasForeignKey(e => e.SolutionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.BillingCycle)
                .WithMany()
                .HasForeignKey(e => e.BillingCycleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TenantSubscription
        modelBuilder.Entity<Domain.Entities.TenantSubscription>(b =>
        {
            b.ToTable("TenantSubscriptions");
            b.HasKey(e => e.Id);
            b.Ignore(e => e.Tenant);
            b.Ignore(e => e.Solution);
            b.Ignore(e => e.SubscriptionPlan);
            b.Ignore(e => e.Status);
        });

        // RefreshToken — needed by VerifyEmail handler
        modelBuilder.Entity<Domain.Entities.RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
            b.HasKey(e => e.Id);
            b.Property(e => e.UserId).IsRequired();
            b.Property(e => e.Token).IsRequired().HasMaxLength(500);
            b.Property(e => e.ExpiresAt).IsRequired();
            b.Ignore(e => e.User);
        });

        // Ignore all remaining entities not needed for these tests
        modelBuilder.Ignore<Domain.Entities.Country>();
        modelBuilder.Ignore<Domain.Entities.Currency>();
        modelBuilder.Ignore<Domain.Entities.Gender>();
        modelBuilder.Ignore<Domain.Entities.IdType>();
        modelBuilder.Ignore<Domain.Entities.Language>();
        modelBuilder.Ignore<Domain.Entities.MenuGroup>();
        modelBuilder.Ignore<Domain.Entities.MenuOption>();
        modelBuilder.Ignore<Domain.Entities.Module>();
        modelBuilder.Ignore<Domain.Entities.Municipality>();
        modelBuilder.Ignore<Domain.Entities.OptionAction>();
        modelBuilder.Ignore<Domain.Entities.Province>();
        modelBuilder.Ignore<Domain.Entities.RoleTemplate>();
        modelBuilder.Ignore<Domain.Entities.Role>();
        modelBuilder.Ignore<Domain.Entities.RoleMenuOption>();
        modelBuilder.Ignore<Domain.Entities.RoleMenuOptionTemplate>();
        modelBuilder.Ignore<Domain.Entities.RoleOptionAction>();
        modelBuilder.Ignore<Domain.Entities.RoleOptionActionTemplate>();
        modelBuilder.Ignore<Domain.Entities.SolutionMenuOption>();
        modelBuilder.Ignore<Domain.Entities.UserRole>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditFields()
    {
        var now = _dateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity.IsActive)).CurrentValue = true;
            }
            if (entry.State == EntityState.Modified)
                entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;
        }

        var tenantId = _tenantService.CurrentTenantId;
        var userId = _currentUserService.UserId;
        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property(nameof(TenantEntity.TenantId)).CurrentValue is 0 or null && tenantId.HasValue)
                    entry.Property(nameof(TenantEntity.TenantId)).CurrentValue = tenantId.Value;
                entry.Property(nameof(TenantEntity.CreatedBy)).CurrentValue = userId;
            }
            if (entry.State == EntityState.Modified)
                entry.Property(nameof(TenantEntity.UpdatedBy)).CurrentValue = userId;
        }
    }

    public new DbSet<TEntity> Set<TEntity>() where TEntity : class => base.Set<TEntity>();
}

// ---------------------------------------------------------------
// Stub UnitOfWork — wraps the same DbContext connection
// ---------------------------------------------------------------
public sealed class StubUnitOfWork : Domain.Repositories.IUnitOfWork
{
    private readonly RegistrationTestDbContext _db;
    public StubUnitOfWork(RegistrationTestDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public async Task CommitTransactionAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
    public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Dispose() { }
}

// ---------------------------------------------------------------
// Base class — wires up a fresh SQLite DB per test class
// ---------------------------------------------------------------
public abstract class HandlerTestBase : IDisposable
{
    protected readonly SqliteConnection Connection;
    protected readonly RegistrationTestDbContext Db;
    protected readonly StubPasswordHasher PasswordHasher = new();
    protected readonly StubTokenService TokenService = new();
    protected readonly StubEmailService EmailService = new();
    protected readonly StubCurrentUserService CurrentUser = new();
    protected readonly StubTenantService TenantService = new();
    protected readonly StubRateLimitService RateLimitService = new();
    protected readonly StubUnitOfWork UnitOfWork;

    protected HandlerTestBase()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<RegistrationTestDbContext>()
            .UseSqlite(Connection)
            .Options;

        Db = new RegistrationTestDbContext(options, CurrentUser, TenantService, new StubDateTime());
        Db.Database.EnsureCreated();
        UnitOfWork = new StubUnitOfWork(Db);

        SeedStatuses();
    }

    private void SeedStatuses()
    {
        // Seed TenantStatuses
        var statuses = new[]
        {
            new { Code = "PENDING_EMAIL_VERIFICATION", Name = "Pending Email Verification" },
            new { Code = "PENDING_SUBSCRIPTION",       Name = "Pending Subscription" },
            new { Code = "ACTIVE",                     Name = "Active" },
            new { Code = "SUSPENDED",                  Name = "Suspended" },
            new { Code = "CANCELLED",                  Name = "Cancelled" },
        };
        foreach (var s in statuses)
        {
            Db.Database.ExecuteSqlRaw(
                "INSERT INTO TenantStatuses (Code, Name, Description, IsActive, CreatedAt) VALUES ({0}, {1}, '', 1, {2})",
                s.Code, s.Name, DateTime.UtcNow);
        }

        // Seed SubscriptionStatuses
        var subStatuses = new[]
        {
            new { Code = "TRIAL",     Name = "Trial" },
            new { Code = "ACTIVE",    Name = "Active" },
            new { Code = "PAST_DUE",  Name = "Past Due" },
            new { Code = "SUSPENDED", Name = "Suspended" },
            new { Code = "CANCELLED", Name = "Cancelled" },
            new { Code = "EXPIRED",   Name = "Expired" },
        };
        foreach (var s in subStatuses)
        {
            Db.Database.ExecuteSqlRaw(
                "INSERT INTO SubscriptionStatuses (Code, Name, IsActive, CreatedAt) VALUES ({0}, {1}, 1, {2})",
                s.Code, s.Name, DateTime.UtcNow);
        }
    }

    public void Dispose()
    {
        Db.Dispose();
        Connection.Dispose();
    }
}
