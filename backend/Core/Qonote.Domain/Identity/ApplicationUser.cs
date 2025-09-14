using Microsoft.AspNetCore.Identity;
using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Domain.Identity;

public class ApplicationUser : IdentityUser, IEntityBase<string>
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Properties from IEntityBase
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<NoteGroup> NoteGroups { get; set; } = new List<NoteGroup>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<SectionUIState> SectionUIStates { get; set; } = new List<SectionUIState>();
}
