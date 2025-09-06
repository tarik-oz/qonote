using Microsoft.AspNetCore.Identity;
using Qonote.Core.Domain.Common;

namespace Qonote.Core.Domain.Identity;

public class ApplicationRole : IdentityRole, IEntityBase<string>
{
    // Properties from IEntityBase
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
