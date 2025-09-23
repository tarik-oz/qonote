using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes._Rules;

public sealed class CustomTitleMustBeUniqueForUserOnCreateRule : IBusinessRule<CreateNoteFromYoutubeUrlCommand>
{
    public int Order => 10;

    private readonly IReadRepository<Note, int> _noteRead;
    private readonly ICurrentUserService _currentUser;

    public CustomTitleMustBeUniqueForUserOnCreateRule(IReadRepository<Note, int> noteRead, ICurrentUserService currentUser)
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

        var title = request.CustomTitle?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return Array.Empty<RuleViolation>();
        }

        var existing = await _noteRead.GetAllAsync(
            n => n.UserId == userId && n.CustomTitle.ToLower() == title.ToLower(),
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
