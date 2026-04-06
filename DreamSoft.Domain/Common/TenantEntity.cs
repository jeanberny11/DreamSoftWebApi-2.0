namespace DreamSoft.Domain.Common;

/// <summary>
/// Base entity for tenant-scoped tables with audit trail.
/// Provides tenant isolation (TenantId) and audit trail (CreatedBy, UpdatedBy).
/// Navigation properties to Tenant and User will be re-added once entities are rebuilt.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public int TenantId { get; protected set; }

    public int? CreatedBy { get; protected set; }

    public int? UpdatedBy { get; protected set; }

    protected void InitializeTenantEntity(int tenantId, int? createdBy)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        TenantId = tenantId;
        CreatedBy = createdBy;
        UpdatedBy = null;

        InitializeAudit();
    }

    protected void RecordUpdate(int? userId)
    {
        UpdatedBy = userId;
        MarkAsUpdated();
    }
}
