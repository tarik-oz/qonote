using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;
using Qonote.Core.Application.Features.Notes._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes._Rules;

public sealed class NoDuplicateVideoForUserOnCreateRule : IBusinessRule<CreateNoteFromYoutubeUrlCommand>
{
    public int Order => 20;

    private readonly IReadRepository<Note, int> _noteRead;
    private readonly ICurrentUserService _currentUser;

    public NoDuplicateVideoForUserOnCreateRule(IReadRepository<Note, int> noteRead, ICurrentUserService currentUser)
    {
        _noteRead = noteRead;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<RuleViolation>> CheckAsync(CreateNoteFromYoutubeUrlCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<RuleViolation>();
        }

        var videoId = YouTubeUrlParser.TryExtractVideoId(request.YoutubeUrl);
        if (videoId is null)
        {
            return Array.Empty<RuleViolation>();
        }

        var existing = await _noteRead.GetAllAsync(
            n => n.UserId == userId && n.YoutubeUrl.Contains(videoId),
            cancellationToken);

        if (existing.Any())
        {
            var id = existing.First().Id.ToString();
            return new[]
            {
                new RuleViolation("existingNoteId", id),
                new RuleViolation("YoutubeUrl", "A note for this YouTube video already exists.")
            };
        }

        return Array.Empty<RuleViolation>();
    }
}
