namespace Qonote.Core.Application.Abstractions.Data;

public interface ISectionUiStateStore
{
    Task SetCollapsedAsync(string userId, int sectionId, bool isCollapsed, CancellationToken cancellationToken);
    Task SetCollapsedBatchAsync(string userId, int noteId, IEnumerable<(int SectionId, bool IsCollapsed)> items, CancellationToken cancellationToken);
    Task<HashSet<int>> GetCollapsedSectionIdsAsync(string userId, IEnumerable<int> sectionIds, CancellationToken cancellationToken);
}


