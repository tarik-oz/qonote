using Qonote.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qonote.Domain.Entities
{
    public class SectionUIState
    {
        // TODO: This entity does not have its own primary key in the class.
        // The composite key (UserId, SectionId) will be configured in the DbContext
        // using Fluent API in the OnModelCreating method.
        public bool IsCollapsed { get; set; } = false;

        // Composite Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int SectionId { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Section Section { get; set; } = null!;
    }
}
