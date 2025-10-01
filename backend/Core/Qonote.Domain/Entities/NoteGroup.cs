using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Domain.Entities
{
    public class NoteGroup : EntityBase<int>, ISidebarAffecting
    {
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; } = 0;

        // Foreign Key
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
