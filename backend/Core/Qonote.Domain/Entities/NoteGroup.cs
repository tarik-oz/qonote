using Qonote.Domain.Common;
using Qonote.Domain.Identity;

namespace Qonote.Domain.Entities
{
    public class NoteGroup : EntityBase<int>
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
