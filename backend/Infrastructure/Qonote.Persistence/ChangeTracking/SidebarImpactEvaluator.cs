using Microsoft.EntityFrameworkCore;
using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.ChangeTracking;

public sealed class SidebarImpactEvaluator : ISidebarImpactEvaluator
{
    public HashSet<string> CollectAffectedUserIds(ApplicationDbContext context)
    {
        var userIds = new HashSet<string>();

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged && e.Entity is ISidebarAffecting))
        {
            bool affectsSidebar = false;

            if (entry.Entity is Note)
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                {
                    affectsSidebar = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var affectingProps = new HashSet<string>
                    {
                        nameof(Note.Order),
                        nameof(Note.NoteGroupId),
                        nameof(Note.IsDeleted),
                        nameof(Note.CustomTitle)
                    };

                    affectsSidebar = entry.Properties.Any(p => p.IsModified && affectingProps.Contains(p.Metadata.Name));
                }
            }
            else if (entry.Entity is NoteGroup)
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                {
                    affectsSidebar = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var affectingProps = new HashSet<string>
                    {
                        nameof(NoteGroup.Name),
                        nameof(NoteGroup.Order),
                        nameof(NoteGroup.IsDeleted)
                    };

                    affectsSidebar = entry.Properties.Any(p => p.IsModified && affectingProps.Contains(p.Metadata.Name));
                }
            }
            else
            {
                // For unknown ISidebarAffecting types, be conservative and invalidate
                affectsSidebar = true;
            }

            if (!affectsSidebar)
            {
                continue;
            }

            var userId = ((ISidebarAffecting)entry.Entity).UserId;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                userIds.Add(userId);
            }
        }

        return userIds;
    }
}


