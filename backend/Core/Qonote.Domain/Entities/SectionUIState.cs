using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Domain.Entities
{
    public class SectionUIState
    {
        // Composite Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int SectionId { get; set; }

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public Section Section { get; set; } = null!;
    }
}
