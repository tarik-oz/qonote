using Qonote.Domain.Common;
using Qonote.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qonote.Domain.Entities
{
    public class Section : BaseEntity<int>
    {
        public string Title { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Order { get; set; }
        public SectionType Type { get; set; }

        // Foreign Key
        public int NoteId { get; set; }

        // Navigation Properties
        public virtual Note Note { get; set; } = null!;
        public virtual ICollection<Block> Blocks { get; set; } = new List<Block>();
        public virtual ICollection<SectionUIState> SectionUIStates { get; set; } = new List<SectionUIState>();
    }
}
