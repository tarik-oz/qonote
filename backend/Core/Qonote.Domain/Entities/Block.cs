using Qonote.Domain.Common;
using Qonote.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qonote.Domain.Entities
{
    public class Block : BaseEntity<Guid>
    {
        public string Content { get; set; } = string.Empty;
        public BlockType Type { get; set; }
        public int Order { get; set; }

        // Foreign Key
        public int SectionId { get; set; }

        // Navigation Property
        public virtual Section Section { get; set; } = null!;
    }
}
