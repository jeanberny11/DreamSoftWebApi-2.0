namespace DreamSoft.Domain.Common;

/// <summary>
/// Base entity for tenant-scoped tables with audit trail.
/// Provides tenant + solution isolation (TenantId, SolutionId)
/// and audit trail (CreatedBy, UpdatedBy).
/// All tenant-owned business entities must inherit from this class.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public int TenantId { get; protected set; }

    public int SolutionId { get; protected set; }

    public int? CreatedBy { get; protected set; }

    public int? UpdatedBy { get; protected set; }

    protected void InitializeTenantEntity(int tenantId, int solutionId, int? createdBy)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (solutionId <= 0)
            throw new ArgumentException("Solution ID must be greater than zero", nameof(solutionId));

        TenantId = tenantId;
        SolutionId = solutionId;
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
