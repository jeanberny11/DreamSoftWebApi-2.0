using DreamSoft.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add / update).
/// Allows the tooling to create the DbContext without running the full DI container.
/// Connection string is read from appsettings.json in the API project.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "DreamSoft.Api");

        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new NullCurrentUserService(),
            new NullTenantService(),
            new NullDateTime());
    }
}

// ── Design-time null stubs ────────────────────────────────────────────────────

file sealed class NullCurrentUserService : ICurrentUserService
{
    public int? UserId         => null;
    public int? TenantId       => null;
    public int? SolutionId     => null;
    public string? Email       => null;
    public string? Username    => null;
    public bool IsAdmin        => false;
    public bool IsAuthenticated  => false;
    public bool IsTenantIdentity => false;
    public bool IsUserIdentity   => false;
    public string? IpAddress     => null;
    public string? Subdomain     => null;
}

file sealed class NullTenantService : ITenantService
{
    public int? CurrentTenantId   => null;
    public int? CurrentSolutionId => null;
}

file sealed class NullDateTime : IDateTime
{
    public DateTime Now     => DateTime.UtcNow;
    public DateTime UtcNow  => DateTime.UtcNow;
}
