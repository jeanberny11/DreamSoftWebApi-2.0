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
    // CORE ENTITIES (2)
    // ============================================
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }

    // ============================================
    // LOOKUP ENTITIES (18)
    // ============================================
    
    // System Lookups (6)
    DbSet<TenantStatus> TenantStatuses { get; }
    DbSet<Gender> Genders { get; }
    DbSet<IdType> IdTypes { get; }
    DbSet<Language> Languages { get; }
    DbSet<SubscriptionTier> SubscriptionTiers { get; }
    DbSet<TaxClassification> TaxClassifications { get; }

    // Geographic Lookups (3)
    DbSet<Country> Countries { get; }
    DbSet<Province> Provinces { get; }
    DbSet<Municipality> Municipalities { get; }

    // From ENUM Lookups (6)
    DbSet<CustomerType> CustomerTypes { get; }
    DbSet<CustomerStatus> CustomerStatuses { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<ProductStatus> ProductStatuses { get; }
    DbSet<PermissionAction> PermissionActions { get; }
    DbSet<BillingCycle> BillingCycles { get; }

    // Menu System Lookups (3)
    DbSet<Module> Modules { get; }
    DbSet<MenuGroup> MenuGroups { get; }
    DbSet<MenuItem> MenuItems { get; }

    // ============================================
    // SYSTEM ENTITIES (4)
    // ============================================
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<TenantSubscription> TenantSubscriptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Permission> Permissions { get; }

    // ============================================
    // TENANT ENTITIES (8)
    // ============================================
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Customer> Customers { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }

    // ============================================
    // DATABASE OPERATIONS
    // ============================================
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}