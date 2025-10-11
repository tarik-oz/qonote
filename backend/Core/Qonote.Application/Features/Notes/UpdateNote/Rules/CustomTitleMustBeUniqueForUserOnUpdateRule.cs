using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.UpdateNote.Rules;

public sealed class CustomTitleMustBeUniqueForUserOnUpdateRule : IBusinessRule<UpdateNoteCommand>
{
    public int Order => 10;

    private readonly IReadRepository<Note, int> _noteRead;
    private readonly ICurrentUserService _currentUser;

    public CustomTitleMustBeUniqueForUserOnUpdateRule(
        IReadRepository<Note, int> noteRead,
        ICurrentUserService currentUser)
    {
        _noteRead = noteRead;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<RuleViolation>> CheckAsync(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<RuleViolation>();
        }

        if (string.IsNullOrWhiteSpace(request.CustomTitle))
        {
            return Array.Empty<RuleViolation>();
        }

        var normalized = request.CustomTitle.Trim().ToLower();
        var existing = await _noteRead.GetAllAsync(
            n => n.UserId == userId && n.Id != request.Id && n.CustomTitle.ToLower() == normalized,
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
