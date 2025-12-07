namespace DreamSoft.Domain.Entities
{
    public abstract class BaseEntity<TId> where TId : notnull
    {
        public TId Id { get; protected set; } = default!;
        public DateTime CreatedAt { get; protected set; }
        public bool IsActive { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
    }
}