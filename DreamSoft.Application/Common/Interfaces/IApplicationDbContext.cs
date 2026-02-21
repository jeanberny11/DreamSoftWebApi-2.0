using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Application database context interface.
/// Provides access to all entity sets for the multi-tenant ERP system.
/// </summary>
public interface IApplicationDbContext
{
    // ============================================
    // LOOKUP ENTITIES
    // ============================================

    /// <summary>Billing cycle options (monthly, quarterly, annual)</summary>
    DbSet<BillingCycle> BillingCycles { get; }

    /// <summary>Countries lookup table</summary>
    DbSet<Country> Countries { get; }

    /// <summary>Currencies lookup table</summary>
    DbSet<Currency> Currencies { get; }

    /// <summary>Gender types</summary>
    DbSet<Gender> Genders { get; }

    /// <summary>ID/Document types by country</summary>
    DbSet<IdType> IdTypes { get; }

    /// <summary>Supported languages</summary>
    DbSet<Language> Languages { get; }

    /// <summary>Menu groupings for UI organization</summary>
    DbSet<MenuGroup> MenuGroups { get; }

    /// <summary>Menu options/items in the application</summary>
    DbSet<MenuOption> MenuOptions { get; }

    /// <summary>Application modules</summary>
    DbSet<Module> Modules { get; }

    /// <summary>Municipalities within provinces</summary>
    DbSet<Municipality> Municipalities { get; }

    /// <summary>Permission action types (view, create, edit, delete, etc.)</summary>
    DbSet<OptionAction> OptionActions { get; }

    /// <summary>Provinces/states within countries</summary>
    DbSet<Province> Provinces { get; }

    /// <summary>Business solutions (POS, Restaurant, Financial, etc.)</summary>
    DbSet<Solution> Solutions { get; }

    /// <summary>Subscription status types (Active, Trial, Suspended, etc.)</summary>
    DbSet<SubscriptionStatus> SubscriptionStatuses { get; }

    /// <summary>Tenant status types</summary>
    DbSet<TenantStatus> TenantStatuses { get; }

    // ============================================
    // BUSINESS ENTITIES
    // ============================================

    /// <summary>Role templates for quick role setup</summary>
    DbSet<RoleTemplate> RoleTemplates { get; }

    /// <summary>Subscription plans combining solutions and billing cycles</summary>
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }

    /// <summary>Tenant organizations</summary>
    DbSet<Tenant> Tenants { get; }

    /// <summary>Tenant subscription history</summary>
    DbSet<TenantSubscription> TenantSubscriptions { get; }

    /// <summary>Users within tenants</summary>
    DbSet<User> Users { get; }

    /// <summary>Roles within tenants</summary>
    DbSet<Role> Roles { get; }

    // ============================================
    // JUNCTION/RELATIONSHIP TABLES
    // ============================================

    /// <summary>Solution to menu option assignments (many-to-many)</summary>
    DbSet<SolutionMenuOption> SolutionMenuOptions { get; }

    /// <summary>Role to menu option assignments (many-to-many)</summary>
    DbSet<RoleMenuOption> RoleMenuOptions { get; }

    /// <summary>Role template to menu option assignments (many-to-many)</summary>
    DbSet<RoleMenuOptionTemplate> RoleMenuOptionTemplates { get; }

    /// <summary>Role permissions for specific menu option actions</summary>
    DbSet<RoleOptionAction> RoleOptionActions { get; }

    /// <summary>Role template permissions for specific menu option actions</summary>
    DbSet<RoleOptionActionTemplate> RoleOptionActionTemplates { get; }

    /// <summary>User-role assignments (many-to-many with extra payload)</summary>
    DbSet<UserRole> UserRoles { get; }

    // ============================================
    // DATABASE OPERATIONS
    // ============================================
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
