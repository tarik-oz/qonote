using Qonote.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qonote.Domain.Identity
{
    // TODO: Uncomment the inheritance from IdentityUser when integrating with ASP.NET Core Identity
    public class ApplicationUser //: IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<NoteGroup> NoteGroups { get; set; } = new List<NoteGroup>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<SectionUIState> SectionUIStates { get; set; } = new List<SectionUIState>();
    }
}
