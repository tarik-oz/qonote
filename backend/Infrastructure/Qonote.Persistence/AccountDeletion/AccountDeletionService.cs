using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Caching;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Storage;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Identity;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.AccountDeletion;

public sealed class AccountDeletionService : IAccountDeletionService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorage;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly IUnitOfWork _uow;

    public AccountDeletionService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IFileStorageService fileStorage,
        ICacheInvalidationService cacheInvalidation,
        IUnitOfWork uow)
    {
        _db = db;
        _userManager = userManager;
        _fileStorage = fileStorage;
        _cacheInvalidation = cacheInvalidation;
        _uow = uow;
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        // gather blobs to delete after DB commit
        var thumbnailUrls = await _db.Set<Note>()
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted && !string.IsNullOrWhiteSpace(n.ThumbnailUrl))
            .Select(n => n.ThumbnailUrl!)
            .ToListAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return; // idempotent
        }
        var profileUrl = user.ProfileImageUrl;

        // Remove SectionUIState records
        var uiStates = _db.Set<SectionUIState>().Where(s => s.UserId == userId);
        _db.RemoveRange(uiStates);

        // Blocks -> Sections -> Notes -> NoteGroups
        var userNoteIds = await _db.Set<Note>()
            .Where(n => n.UserId == userId)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (userNoteIds.Count > 0)
        {
            var sectionIds = await _db.Set<Section>()
                .Where(s => userNoteIds.Contains(s.NoteId))
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            if (sectionIds.Count > 0)
            {
                var blocks = _db.Set<Block>().Where(b => sectionIds.Contains(b.SectionId));
                _db.RemoveRange(blocks);
                var sections = _db.Set<Section>().Where(s => sectionIds.Contains(s.Id));
                _db.RemoveRange(sections);
            }

            var notes = _db.Set<Note>().Where(n => userNoteIds.Contains(n.Id));
            _db.RemoveRange(notes);
        }

        var groups = _db.Set<NoteGroup>().Where(g => g.UserId == userId);
        _db.RemoveRange(groups);

        // Subscriptions/Payments
        var payments = _db.Set<Payment>().Where(p => p.UserId == userId);
        _db.RemoveRange(payments);
        var subs = _db.Set<UserSubscription>().Where(s => s.UserId == userId);
        _db.RemoveRange(subs);

        // Finally the user
        _db.Remove(user);

        // Commit via UnitOfWork to trigger cross-cutting behaviors (e.g., sidebar cache invalidation)
        await _uow.SaveChangesAsync(cancellationToken);

        // Best-effort: delete blobs
        foreach (var url in thumbnailUrls)
        {
            try { await _fileStorage.DeleteAsync(url, "thumbnails"); } catch { /* best-effort */ }
        }
        if (!string.IsNullOrWhiteSpace(profileUrl))
        {
            try { await _fileStorage.DeleteAsync(profileUrl!, "profile-pictures"); } catch { /* best-effort */ }
        }

        // Invalidate remaining caches explicitly (Me). Sidebar invalidation is handled by UnitOfWork.
        await _cacheInvalidation.RemoveMeAsync(userId, cancellationToken);
    }
}


