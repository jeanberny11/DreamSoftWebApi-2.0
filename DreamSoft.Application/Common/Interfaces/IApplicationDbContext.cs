using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Module = DreamSoft.Domain.Entities.Module;

namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Application database context interface.
/// </summary>
public interface IApplicationDbContext
{
    // =====================================================================
    // GLOBAL — LOOKUP ENTITIES
    // =====================================================================
    DbSet<BillingCycle> BillingCycles { get; }
    DbSet<Country> Countries { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Gender> Genders { get; }
    DbSet<IdType> IdTypes { get; }
    DbSet<Language> Languages { get; }
    DbSet<MenuGroup> MenuGroups { get; }
    DbSet<MenuOption> MenuOptions { get; }
    DbSet<Module> Modules { get; }
    DbSet<Municipality> Municipalities { get; }
    DbSet<OptionAction> OptionActions { get; }
    DbSet<Province> Provinces { get; }
    DbSet<SubscriptionStatus> SubscriptionStatuses { get; }
    DbSet<TenantStatus> TenantStatuses { get; }
    DbSet<CustomerType> CustomerTypes { get; }
    DbSet<CustomerStatus> CustomerStatuses { get; }
    DbSet<TaxClassification> TaxClassifications { get; }

    // =====================================================================
    // GLOBAL — BUSINESS ENTITIES
    // =====================================================================
    DbSet<Solution> Solutions { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<PlanPrice> PlanPrices { get; }
    DbSet<PlanLimit> PlanLimits { get; }
    DbSet<PlanMenuOption> PlanMenuOptions { get; }
    DbSet<RoleTemplate> RoleTemplates { get; }
    DbSet<RoleMenuOptionTemplate> RoleMenuOptionTemplates { get; }
    DbSet<RoleOptionActionTemplate> RoleOptionActionTemplates { get; }

    // =====================================================================
    // TENANT-SCOPED
    // =====================================================================
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantSubdomain> TenantSubdomains { get; }
    DbSet<TenantSubscription> TenantSubscriptions { get; }
    DbSet<TenantRefreshToken> TenantRefreshTokens { get; }
    DbSet<TenantRegistrationToken> TenantRegistrationTokens { get; }
    DbSet<SubscriptionInvoice> SubscriptionInvoices { get; }
    DbSet<SubscriptionPayment> SubscriptionPayments { get; }
    DbSet<SubscriptionCancellationLog> SubscriptionCancellationLogs { get; }

    // =====================================================================
    // TENANT + SOLUTION SCOPED
    // =====================================================================
    DbSet<User> Users { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<RoleMenuOption> RoleMenuOptions { get; }
    DbSet<RoleOptionAction> RoleOptionActions { get; }
    DbSet<Customer> Customers { get; }

    // =====================================================================
    // INFRASTRUCTURE
    // =====================================================================
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
