namespace Qonote.Core.Domain.Common
{
    public interface IEntityBase<TKey> where TKey : notnull
    {
        public TKey Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
