using Qonote.Domain.Common;
using Qonote.Domain.Enums;

namespace Qonote.Domain.Entities
{
    public class Block : EntityBase<Guid>
    {
        public string Content { get; set; } = string.Empty;
        public BlockType Type { get; set; }
        public int Order { get; set; }

        // Foreign Key
        public int SectionId { get; set; }

        // Navigation Property
        public Section Section { get; set; } = null!;
    }
}
