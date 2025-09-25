using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.YouTube;
using Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;
using Qonote.Core.Application.Features.Notes._Shared;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes._Rules;

public sealed class CustomTitleMustBeUniqueForUserOnCreateRule : IBusinessRule<CreateNoteFromYoutubeUrlCommand>
{
    public int Order => 10;

    private readonly IReadRepository<Note, int> _noteRead;
    private readonly ICurrentUserService _currentUser;
    private readonly IYouTubeMetadataService _youTube;

    public CustomTitleMustBeUniqueForUserOnCreateRule(
        IReadRepository<Note, int> noteRead,
        ICurrentUserService currentUser,
        IYouTubeMetadataService youTube)
    {
        _noteRead = noteRead;
        _currentUser = currentUser;
        _youTube = youTube;
    }

    public async Task<IEnumerable<RuleViolation>> CheckAsync(CreateNoteFromYoutubeUrlCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<RuleViolation>();
        }

        var title = request.CustomTitle?.Trim();
        string? titleToCheck = null;

        if (!string.IsNullOrWhiteSpace(title))
        {
            titleToCheck = title;
        }
        else
        {
            // No custom title provided, derive from YouTube metadata
            var videoId = YouTubeUrlParser.TryExtractVideoId(request.YoutubeUrl);
            if (videoId is null)
            {
                return Array.Empty<RuleViolation>();
            }

            var meta = await _youTube.GetVideoMetadataAsync(videoId, cancellationToken);
            // Pass metadata along the pipeline to avoid duplicate calls in handler
            request.Metadata ??= meta;
            var derived = meta.Title?.Trim();
            if (!string.IsNullOrWhiteSpace(derived))
            {
                titleToCheck = derived;
            }
        }

        if (string.IsNullOrWhiteSpace(titleToCheck))
        {
            return Array.Empty<RuleViolation>();
        }

        var existing = await _noteRead.GetAllAsync(
            n => n.UserId == userId && n.CustomTitle.ToLower() == titleToCheck.ToLower(),
            cancellationToken);

        if (existing.Any())
        {
            var id = existing.First().Id.ToString();
            return new[]
            {
                new RuleViolation("CustomTitle", "You already have a note with this title."),
                new RuleViolation("existingNoteId", id)
            };
        }

        return Array.Empty<RuleViolation>();
    }
}
