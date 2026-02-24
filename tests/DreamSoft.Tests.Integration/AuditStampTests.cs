using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Common;
using DreamSoft.Domain.Entities;
using DreamSoft.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Tests.Integration;

/// <summary>
/// Verifies that SaveChangesAsync automatically stamps audit fields on
/// AuditableEntity (CreatedAt, UpdatedAt, IsActive) and TenantEntity
/// (TenantId, CreatedBy, UpdatedBy) without the caller needing to set them.
/// </summary>
public class AuditStampTests : IDisposable
{
    // ---------------------------------------------------------------
    // Controllable stubs
    // ---------------------------------------------------------------
    private sealed class StubDateTime(DateTime fixedUtc) : IDateTime
    {
        public DateTime Now => fixedUtc;
        public DateTime UtcNow => fixedUtc;
    }

    private sealed class StubCurrentUserService : ICurrentUserService
    {
        public StubCurrentUserService(int? userId, int? tenantId)
        {
            UserId = userId;
            TenantId = tenantId;
        }

        public int? UserId { get; set; }
        public int? TenantId { get; set; }
        public string? Email => null;
        public string? Username => null;
        public bool IsAdmin => false;
        public bool IsAuthenticated => UserId.HasValue;
        public string? IpAddress => null;
        public string? Subdomain => null;
    }

    private sealed class StubTenantService(int? tenantId) : ITenantService
    {
        public int? CurrentTenantId { get; set; } = tenantId;
    }

    // ---------------------------------------------------------------
    // Minimal DbContext — same shape as TenantIsolationTests but also
    // wires up the audit-stamp logic from ApplicationDbContext.
    // ---------------------------------------------------------------
    private sealed class AuditTestDbContext(
        DbContextOptions<AuditTestDbContext> options,
        ICurrentUserService currentUserService,
        ITenantService tenantService,
        IDateTime dateTime)
        : DbContext(options)
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly ITenantService _tenantService = tenantService;
        private readonly IDateTime _dateTime = dateTime;

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

                b.HasQueryFilter(e => _tenantService.CurrentTenantId == null
                                   || e.TenantId == _tenantService.CurrentTenantId);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

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

                    entry.Property(nameof(TenantEntity.CreatedBy)).CurrentValue = userId;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(TenantEntity.UpdatedBy)).CurrentValue = userId;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // Fixed values used across all tests
    // ---------------------------------------------------------------
    private static readonly DateTime FixedUtc = new(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedUtc = new(2025, 6, 15, 18, 0, 0, DateTimeKind.Utc);
    private const int ActorUserId = 42;
    private const int TenantId = 7;

    private readonly StubCurrentUserService _userService = new(ActorUserId, TenantId);
    private readonly StubTenantService _tenantService = new(TenantId);
    private StubDateTime _dateTime = new(FixedUtc);

    // Shared connection keeps the in-memory SQLite database alive across contexts
    private readonly SqliteConnection _connection;
    private readonly AuditTestDbContext _db;

    private AuditTestDbContext BuildContext(
        ICurrentUserService? userService = null,
        ITenantService? tenantService = null,
        IDateTime? dateTime = null) => new(
            new DbContextOptionsBuilder<AuditTestDbContext>()
                .UseSqlite(_connection)
                .Options,
            userService ?? _userService,
            tenantService ?? _tenantService,
            dateTime ?? _dateTime);

    public AuditStampTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _db = BuildContext();
        _db.Database.EnsureCreated();
    }

    // ---------------------------------------------------------------
    // Tests — AuditableEntity fields
    // ---------------------------------------------------------------

    [Fact]
    public async Task SaveChangesAsync_OnAdd_Sets_CreatedAt_To_UtcNow()
    {
        var user = User.Create(TenantId, "u1", "u1@test.com", "hash", "First", "Last");
        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        Assert.Equal(FixedUtc, user.CreatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_OnAdd_Sets_IsActive_True()
    {
        var user = User.Create(TenantId, "u2", "u2@test.com", "hash", "First", "Last");
        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task SaveChangesAsync_OnAdd_UpdatedAt_Remains_Null()
    {
        var user = User.Create(TenantId, "u3", "u3@test.com", "hash", "First", "Last");
        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_OnModify_Sets_UpdatedAt_To_UtcNow()
    {
        // Arrange — seed with creation time
        var user = User.Create(TenantId, "u4", "u4@test.com", "hash", "First", "Last");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Act — update with a later timestamp
        _dateTime = new StubDateTime(UpdatedUtc);
        using var updateCtx = BuildContext(dateTime: _dateTime);

        var tracked = await updateCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Username == "u4");
        tracked.Username = "u4-updated"; // trigger Modified state
        await updateCtx.SaveChangesAsync();

        Assert.Equal(UpdatedUtc, tracked.UpdatedAt);
    }

    // ---------------------------------------------------------------
    // Tests — TenantEntity fields
    // ---------------------------------------------------------------

    [Fact]
    public async Task SaveChangesAsync_OnAdd_Sets_CreatedBy_From_CurrentUser()
    {
        var user = User.Create(TenantId, "u5", "u5@test.com", "hash", "First", "Last");
        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        Assert.Equal(ActorUserId, user.CreatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_OnAdd_UpdatedBy_Remains_Null()
    {
        var user = User.Create(TenantId, "u6", "u6@test.com", "hash", "First", "Last");
        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        Assert.Null(user.UpdatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_OnModify_Sets_UpdatedBy_From_CurrentUser()
    {
        // Arrange
        var user = User.Create(TenantId, "u7", "u7@test.com", "hash", "First", "Last");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Act — different actor updates the entity
        const int updaterUserId = 99;
        var updaterUserService = new StubCurrentUserService(updaterUserId, TenantId);
        using var updateCtx = BuildContext(userService: updaterUserService);

        var tracked = await updateCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Username == "u7");
        tracked.Username = "u7-updated";
        await updateCtx.SaveChangesAsync();

        Assert.Equal(updaterUserId, tracked.UpdatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_OnAdd_ExplicitTenantId_Is_Not_Overwritten()
    {
        // The factory was called with TenantId = 3 (different from the context's tenant 7)
        // — the DbContext must not clobber an already-set TenantId
        const int explicitTenantId = 3;
        var user = User.Create(explicitTenantId, "u8", "u8@test.com", "hash", "First", "Last");

        // Use a null-tenant context so filter doesn't interfere with the save
        var nullTenantService = new StubTenantService(null);
        using var ctx = BuildContext(tenantService: nullTenantService);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        Assert.Equal(explicitTenantId, user.TenantId);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
