namespace DreamSoft.Domain.Common;

/// <summary>
/// Base entity for tenant-scoped tables with audit trail
/// Provides tenant isolation (tenant_id) and audit trail (created_by, updated_by)
/// All data is isolated per tenant - cannot access other tenant's data
/// Examples: Customers, Products, Roles, Users
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    /// <summary>
    /// Foreign key to tenant - provides multi-tenant isolation
    /// Every record belongs to exactly one tenant
    /// </summary>
    public int TenantId { get; protected set; }

    /// <summary>
    /// User who created this entity
    /// Nullable because:
    /// - First admin user is created by system (no user context yet)
    /// - Legacy data may not have this information
    /// </summary>
    public int? CreatedBy { get; protected set; }

    /// <summary>
    /// User who last updated this entity
    /// Nullable because:
    /// - Entity may never be updated after creation
    /// - User table doesn't track who updated the user
    /// - Legacy data may not have this information
    /// </summary>
    public int? UpdatedBy { get; protected set; }

    // Navigation properties
    /// <summary>
    /// Reference to the tenant that owns this entity
    /// </summary>
    public Entities.Tenant Tenant { get; private set; } = null!;

    /// <summary>
    /// Reference to the user who created this entity
    /// Null if created by system or legacy data
    /// </summary>
    public Entities.User? CreatedByUser { get; private set; }

    /// <summary>
    /// Reference to the user who last updated this entity
    /// Null if never updated or legacy data
    /// </summary>
    public Entities.User? UpdatedByUser { get; private set; }

    /// <summary>
    /// Initializes tenant-specific fields
    /// Called by child entity factory methods
    /// </summary>
    /// <param name="tenantId">ID of the tenant that owns this entity</param>
    /// <param name="createdBy">ID of the user creating this entity (null for system-created entities)</param>
    protected void InitializeTenantEntity(int tenantId, int? createdBy)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        TenantId = tenantId;
        CreatedBy = createdBy;
        UpdatedBy = null; // Initially null until first update

        InitializeAudit(); // Initialize base audit fields
    }

    /// <summary>
    /// Records who updated the entity
    /// Call this in any method that modifies tenant entity
    /// </summary>
    /// <param name="userId">ID of the user making the update</param>
    protected void RecordUpdate(int? userId)
    {
        UpdatedBy = userId;
        MarkAsUpdated();
    }
}
