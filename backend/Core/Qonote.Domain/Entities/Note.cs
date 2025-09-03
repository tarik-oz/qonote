using Qonote.Domain.Common;
using Qonote.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Qonote.Domain.Entities
{
    public class Note : BaseEntity<int>
    {
        public string CustomTitle { get; set; } = string.Empty;
        public string YoutubeUrl { get; set; } = string.Empty;
        public string VideoTitle { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public TimeSpan VideoDuration { get; set; }
        public string? UserLink1 { get; set; }
        public string? UserLink2 { get; set; }
        public string? UserLink3 { get; set; }
        public bool IsPublic { get; set; } = false;
        public Guid? PublicShareGuid { get; set; }
        public int Order { get; set; } = 0;

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int? NoteGroupId { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual NoteGroup? NoteGroup { get; set; }
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
