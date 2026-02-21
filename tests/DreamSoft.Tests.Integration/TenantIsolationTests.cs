using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Verifies that HasQueryFilter on TenantEntity DbSets prevents cross-tenant data leakage.
/// Uses SQLite in-memory to avoid requiring a real database.
/// </summary>
public class TenantIsolationTests : IDisposable
{
    // ---------------------------------------------------------------
    // Minimal ITenantService stub — controls which tenant is "active"
    // ---------------------------------------------------------------
    private sealed class StubTenantService(int? tenantId) : ITenantService
    {
        public int? CurrentTenantId { get; set; } = tenantId;
    }

    private sealed class StubCurrentUserService : ICurrentUserService
    {
        public int? UserId => null;
        public int? TenantId => null;
        public string? Email => null;
        public string? Username => null;
        public bool IsAdmin => false;
        public bool IsAuthenticated => false;
        public string? IpAddress => null;
        public string? Subdomain => null;
    }

    private sealed class StubDateTime : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
        public DateTime UtcNow => DateTime.UtcNow;
    }

    // ---------------------------------------------------------------
    // Minimal DbContext that only maps User (the TenantEntity under test)
    // without pulling in Npgsql-specific config or owned JSON types.
    // ---------------------------------------------------------------
    private sealed class TestDbContext(
        DbContextOptions<TestDbContext> options,
        ITenantService tenantService)
        : DbContext(options)
    {
        private readonly ITenantService _tenantService = tenantService;

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(e => e.Id);
                b.Property(e => e.TenantId).IsRequired();
                b.Property(e => e.Username).IsRequired().HasMaxLength(100);
                b.Property(e => e.Email).IsRequired().HasMaxLength(200);
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

                // This is the filter under test
                b.HasQueryFilter(e => _tenantService.CurrentTenantId == null
                                   || e.TenantId == _tenantService.CurrentTenantId);
            });
        }
    }

    // ---------------------------------------------------------------
    // Test helpers
    // ---------------------------------------------------------------
    private readonly StubTenantService _tenantService = new(null);
    private readonly TestDbContext _db;

    public TenantIsolationTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _db = new TestDbContext(options, _tenantService);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        // Seed: two users for Tenant A, one user for Tenant B
        var userA1 = User.Create(tenantId: 1, username: "alice", email: "alice@a.com",
            passwordHash: "hash1", firstName: "Alice", lastName: "A");
        var userA2 = User.Create(tenantId: 1, username: "bob", email: "bob@a.com",
            passwordHash: "hash2", firstName: "Bob", lastName: "A");
        var userB1 = User.Create(tenantId: 2, username: "charlie", email: "charlie@b.com",
            passwordHash: "hash3", firstName: "Charlie", lastName: "B");

        _db.Users.AddRange(userA1, userA2, userB1);
        _db.SaveChanges();
    }

    // ---------------------------------------------------------------
    // Tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task Users_Query_For_TenantA_DoesNot_Return_TenantB_Users()
    {
        _tenantService.CurrentTenantId = 1;

        var users = await _db.Users.ToListAsync();

        Assert.Equal(2, users.Count);
        Assert.All(users, u => Assert.Equal(1, u.TenantId));
        Assert.DoesNotContain(users, u => u.TenantId == 2);
    }

    [Fact]
    public async Task Users_Query_For_TenantB_DoesNot_Return_TenantA_Users()
    {
        _tenantService.CurrentTenantId = 2;

        var users = await _db.Users.ToListAsync();

        Assert.Single(users);
        Assert.Equal(2, users[0].TenantId);
        Assert.Equal("charlie", users[0].Username);
    }

    [Fact]
    public async Task Users_Query_With_Null_TenantId_Returns_All_Users()
    {
        _tenantService.CurrentTenantId = null;

        var users = await _db.Users.ToListAsync();

        Assert.Equal(3, users.Count);
    }

    [Fact]
    public async Task IgnoreQueryFilters_Bypasses_Tenant_Filter()
    {
        _tenantService.CurrentTenantId = 1;

        var users = await _db.Users.IgnoreQueryFilters().ToListAsync();

        Assert.Equal(3, users.Count);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }
}
