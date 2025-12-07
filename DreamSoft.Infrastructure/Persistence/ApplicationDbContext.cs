using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using DreamSoft.Infrastructure.Persistence.Configurations;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new TenantStatusConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GenderConfiguration());
        modelBuilder.ApplyConfiguration(new IdTypeConfiguration());
        modelBuilder.ApplyConfiguration(new LanguageConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
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
    public string? GetCurrentUserId()
    {
        return _currentUserService.UserId;
    }
}