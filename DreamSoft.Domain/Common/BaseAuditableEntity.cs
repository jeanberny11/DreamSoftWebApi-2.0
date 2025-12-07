namespace DreamSoft.Domain.Common;

/// <summary>
/// Base entity with audit fields (temporal tracking and status)
/// Provides created_at, updated_at, and is_active fields
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Timestamp when the entity was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Timestamp when the entity was last updated (UTC)
    /// Nullable because not all entities are updated immediately after creation
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Indicates if the entity is active in the system
    /// Soft delete pattern - inactive entities are not shown in normal queries
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Initializes audit fields with current UTC timestamp
    /// Called by child entity factory methods
    /// </summary>
    protected void InitializeAudit()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null; // Initially null
        IsActive = true;
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp to current UTC time
    /// Call this in any method that modifies the entity
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the entity (soft un-delete)
    /// </summary>
    public virtual void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the entity (soft delete)
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}