namespace Qonote.Core.Domain.Common
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey> where TKey : notnull
    {
        public TKey Id { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
