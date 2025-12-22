using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DreamSoft.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService,
    IDateTime dateTime) : DbContext(options), IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTime _dateTime = dateTime;

    // DbSets will be added here as we create entities
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantStatus> TenantStatuses => Set<TenantStatus>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<IdType> IdTypes => Set<IdType>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Geographic Lookup Entities
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Municipality> Municipalities => Set<Municipality>();

    // Enum-based Lookup Entities
    public DbSet<CustomerType> CustomerTypes => Set<CustomerType>();
    public DbSet<CustomerStatus> CustomerStatuses => Set<CustomerStatus>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<ProductStatus> ProductStatuses => Set<ProductStatus>();
    public DbSet<PermissionAction> PermissionActions => Set<PermissionAction>();
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();

    // Other Lookup Entities
    public DbSet<SubscriptionTier> SubscriptionTiers => Set<SubscriptionTier>();
    public DbSet<TaxClassification> TaxClassifications => Set<TaxClassification>();

    // System Entities
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Domain.Entities.Module> Modules => Set<Domain.Entities.Module>();
    public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RoleTemplate> RoleTemplates => Set<RoleTemplate>();
    public DbSet<RoleMenuItemTemplate> RoleMenuItemTemplates => Set<RoleMenuItemTemplate>();
    public DbSet<RoleMenuActionTemplate> RoleMenuActionTemplates => Set<RoleMenuActionTemplate>();

    // Tenant Entities
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleMenuItem> RoleMenuItems => Set<RoleMenuItem>();
    public DbSet<RoleMenuItemAction> RoleMenuItemActions => Set<RoleMenuItemAction>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // FIXED: Use ONLY assembly scanning to avoid duplicate configuration
        // This will automatically find and apply all IEntityTypeConfiguration<T> classes
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
