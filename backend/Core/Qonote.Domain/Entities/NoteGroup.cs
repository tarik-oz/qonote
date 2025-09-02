using Qonote.Domain.Common;
using Qonote.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qonote.Domain.Entities
{
    public class NoteGroup : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;

        // Foreign Key
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
