using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Module = DreamSoft.Domain.Entities.Module;

namespace DreamSoft.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService,
    IDateTime dateTime) : DbContext(options), IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTime _dateTime = dateTime;

    // ============================================
    // LOOKUP ENTITIES
    // ============================================

    /// <summary>Billing cycle options (monthly, quarterly, annual)</summary>
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();

    /// <summary>Countries lookup table</summary>
    public DbSet<Country> Countries => Set<Country>();

    /// <summary>Currencies lookup table</summary>
    public DbSet<Currency> Currencies => Set<Currency>();

    /// <summary>Gender types</summary>
    public DbSet<Gender> Genders => Set<Gender>();

    /// <summary>ID/Document types by country</summary>
    public DbSet<IdType> IdTypes => Set<IdType>();

    /// <summary>Supported languages</summary>
    public DbSet<Language> Languages => Set<Language>();

    /// <summary>Menu groupings for UI organization</summary>
    public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();

    /// <summary>Menu options/items in the application</summary>
    public DbSet<MenuOption> MenuOptions => Set<MenuOption>();

    /// <summary>Application modules</summary>
    public DbSet<Module> Modules => Set<Module>();

    /// <summary>Municipalities within provinces</summary>
    public DbSet<Municipality> Municipalities => Set<Municipality>();

    /// <summary>Permission action types (view, create, edit, delete, etc.)</summary>
    public DbSet<OptionAction> OptionActions => Set<OptionAction>();

    /// <summary>Provinces/states within countries</summary>
    public DbSet<Province> Provinces => Set<Province>();

    /// <summary>Business solutions (POS, Restaurant, Financial, etc.)</summary>
    public DbSet<Solution> Solutions => Set<Solution>();

    /// <summary>Subscription status types (Active, Trial, Suspended, etc.)</summary>
    public DbSet<SubscriptionStatus> SubscriptionStatuses => Set<SubscriptionStatus>();

    /// <summary>Tenant status types</summary>
    public DbSet<TenantStatus> TenantStatuses => Set<TenantStatus>();

    // ============================================
    // BUSINESS ENTITIES
    // ============================================

    /// <summary>Role templates for quick role setup</summary>
    public DbSet<RoleTemplate> RoleTemplates => Set<RoleTemplate>();

    /// <summary>Subscription plans combining solutions and billing cycles</summary>
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    /// <summary>Tenant organizations</summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>Tenant subscription history</summary>
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    /// <summary>Users within tenants</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Roles within tenants</summary>
    public DbSet<Role> Roles => Set<Role>();

    // ============================================
    // JUNCTION/RELATIONSHIP TABLES
    // ============================================

    /// <summary>Solution to menu option assignments (many-to-many)</summary>
    public DbSet<SolutionMenuOption> SolutionMenuOptions => Set<SolutionMenuOption>();

    /// <summary>Role to menu option assignments (many-to-many)</summary>
    public DbSet<RoleMenuOption> RoleMenuOptions => Set<RoleMenuOption>();

    /// <summary>Role template to menu option assignments (many-to-many)</summary>
    public DbSet<RoleMenuOptionTemplate> RoleMenuOptionTemplates => Set<RoleMenuOptionTemplate>();

    /// <summary>Role permissions for specific menu option actions</summary>
    public DbSet<RoleOptionAction> RoleOptionActions => Set<RoleOptionAction>();

    /// <summary>Role template permissions for specific menu option actions</summary>
    public DbSet<RoleOptionActionTemplate> RoleOptionActionTemplates => Set<RoleOptionActionTemplate>();

    // ============================================
    // DATABASE CONFIGURATION
    // ============================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    /// <summary>
    /// Helper method to get current user ID for audit trail
    /// </summary>
    public int? GetCurrentUserId()
    {
        return _currentUserService.UserId;
    }
}
