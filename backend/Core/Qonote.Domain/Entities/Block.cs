using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Domain.Entities
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
