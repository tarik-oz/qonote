using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Domain.Entities
{
    public class Section : EntityBase<int>
    {
        public string Title { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Order { get; set; }
        public SectionType Type { get; set; }

        // Foreign Key
        public int NoteId { get; set; }

        // Navigation Properties
        public Note Note { get; set; } = null!;
        public ICollection<Block> Blocks { get; set; } = new List<Block>();
        public ICollection<SectionUIState> SectionUIStates { get; set; } = new List<SectionUIState>();
    }
}
