using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Sections._Shared;

public static class SectionTimelineRules
{
    // Minimal timestamped section length
    public static readonly TimeSpan MinLength = TimeSpan.FromSeconds(10);

    public static bool IsWithinVideo(TimeSpan start, TimeSpan end, TimeSpan duration)
        => start >= TimeSpan.Zero && end <= duration && start < end;

    public static bool Overlaps(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB)
        => startA < endB && endA > startB; // touching endpoints is not overlap
}
