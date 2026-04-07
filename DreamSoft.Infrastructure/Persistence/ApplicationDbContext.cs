using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Common;
using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Module = DreamSoft.Domain.Entities.Module;

namespace DreamSoft.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService,
    ITenantService tenantService,
    IDateTime dateTime) : DbContext(options), IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly IDateTime _dateTime = dateTime;

    // =====================================================================
    // GLOBAL — LOOKUP ENTITIES
    // =====================================================================
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<IdType> IdTypes => Set<IdType>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();
    public DbSet<MenuOption> MenuOptions => Set<MenuOption>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<OptionAction> OptionActions => Set<OptionAction>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<SubscriptionStatus> SubscriptionStatuses => Set<SubscriptionStatus>();
    public DbSet<TenantStatus> TenantStatuses => Set<TenantStatus>();

    // =====================================================================
    // GLOBAL — BUSINESS ENTITIES
    // =====================================================================
    public DbSet<Solution> Solutions => Set<Solution>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<PlanPrice> PlanPrices => Set<PlanPrice>();
    public DbSet<PlanLimit> PlanLimits => Set<PlanLimit>();
    public DbSet<PlanMenuOption> PlanMenuOptions => Set<PlanMenuOption>();
    public DbSet<RoleTemplate> RoleTemplates => Set<RoleTemplate>();
    public DbSet<RoleMenuOptionTemplate> RoleMenuOptionTemplates => Set<RoleMenuOptionTemplate>();
    public DbSet<RoleOptionActionTemplate> RoleOptionActionTemplates => Set<RoleOptionActionTemplate>();

    // =====================================================================
    // TENANT-SCOPED
    // =====================================================================
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSubdomain> TenantSubdomains => Set<TenantSubdomain>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<TenantRefreshToken> TenantRefreshTokens => Set<TenantRefreshToken>();
    public DbSet<TenantRegistrationToken> TenantRegistrationTokens => Set<TenantRegistrationToken>();
    public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<SubscriptionCancellationLog> SubscriptionCancellationLogs => Set<SubscriptionCancellationLog>();

    // =====================================================================
    // TENANT + SOLUTION SCOPED
    // =====================================================================
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleMenuOption> RoleMenuOptions => Set<RoleMenuOption>();
    public DbSet<RoleOptionAction> RoleOptionActions => Set<RoleOptionAction>();

    // =====================================================================
    // MODEL CONFIGURATION
    // =====================================================================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ── Tenant-scoped filters ─────────────────────────────────────────
        // Null check allows system-level queries to bypass the filter

        modelBuilder.Entity<TenantSubdomain>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<TenantSubscription>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<TenantRefreshToken>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<TenantRegistrationToken>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<SubscriptionInvoice>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<SubscriptionPayment>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<SubscriptionCancellationLog>()
            .HasQueryFilter(e => _tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId);

        // ── Tenant + Solution scoped filters ─────────────────────────────
        // Both TenantId AND SolutionId must match — critical for solution data isolation
        // RoleMenuOption, RoleOptionAction and UserRefreshToken are excluded —
        // their isolation is inherited through Role and User which are already filtered

        modelBuilder.Entity<User>()
            .HasQueryFilter(e => (_tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId)
                && (_tenantService.CurrentSolutionId == null
                || e.SolutionId == _tenantService.CurrentSolutionId));

        modelBuilder.Entity<Role>()
            .HasQueryFilter(e => (_tenantService.CurrentTenantId == null
                || e.TenantId == _tenantService.CurrentTenantId)
                && (_tenantService.CurrentSolutionId == null
                || e.SolutionId == _tenantService.CurrentSolutionId));

        base.OnModelCreating(modelBuilder);
    }

    // =====================================================================
    // SAVE + AUDIT STAMPING
    // =====================================================================
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically stamps audit fields on all tracked entities before saving.
    /// AuditableEntity: sets CreatedAt / IsActive on Add; UpdatedAt on Modify.
    /// TenantEntity: sets TenantId / CreatedBy on Add; UpdatedBy on Modify.
    /// </summary>
    private void StampAuditFields()
    {
        var now = _dateTime.UtcNow;
        var userId = _currentUserService.UserId;
        var tenantId = _tenantService.CurrentTenantId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity.IsActive)).CurrentValue = true;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property(nameof(TenantEntity.TenantId)).CurrentValue is 0 or null && tenantId.HasValue)
                    entry.Property(nameof(TenantEntity.TenantId)).CurrentValue = tenantId.Value;

                if (userId.HasValue)
                    entry.Property(nameof(TenantEntity.CreatedBy)).CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified && userId.HasValue)
            {
                entry.Property(nameof(TenantEntity.UpdatedBy)).CurrentValue = userId;
            }
        }
    }

    public int? GetCurrentUserId() => _currentUserService.UserId;
}
