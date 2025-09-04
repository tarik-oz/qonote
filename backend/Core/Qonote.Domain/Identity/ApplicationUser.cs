using Microsoft.AspNetCore.Identity;
using Qonote.Domain.Common;
using Qonote.Domain.Entities;

namespace Qonote.Domain.Identity;

public class ApplicationUser : IdentityUser, IEntityBase<string>
{
    public string FullName { get; set; } = string.Empty;
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
