using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.ReorderNotes;

public sealed class ReorderNotesCommandHandler : IRequestHandler<ReorderNotesCommand>
{
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Note, int> _noteWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ReorderNotesCommandHandler> _logger;

    public ReorderNotesCommandHandler(
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Note, int> noteWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<ReorderNotesCommandHandler> logger)
    {
        _noteReader = noteReader;
        _noteWriter = noteWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(ReorderNotesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule
        var ids = request.Items.Select(i => i.Id).ToHashSet();

        // Fetch only user's notes matching ids
        var notes = await _noteReader.GetAllAsync(n => ids.Contains(n.Id) && n.UserId == userId, cancellationToken);
        if (notes.Count != ids.Count)
        {
            // At least one of the provided ids is not owned by user
            throw new NotFoundException("One or more notes not found.");
        }

        // Optional scope by group: if NoteGroupId is provided, ensure all notes belong to that group
        if (request.NoteGroupId.HasValue)
        {
            var targetGroupId = request.NoteGroupId.Value;
            if (notes.Any(n => n.NoteGroupId != targetGroupId))
            {
                throw new ValidationException([new FluentValidation.Results.ValidationFailure("Items", "All notes must belong to the specified NoteGroupId.")]);
            }
        }
        else
        {
            // Ungrouped scope: ensure all are ungrouped
            if (notes.Any(n => n.NoteGroupId != null))
            {
                throw new ValidationException([new FluentValidation.Results.ValidationFailure("Items", "All notes must be ungrouped when NoteGroupId is not provided.")]);
            }
        }

        // Apply given orders
        foreach (var item in request.Items)
        {
            var note = notes.First(n => n.Id == item.Id);
            note.Order = item.Order;
            _noteWriter.Update(note);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        // Normalize orders within scope to contiguous 0..n-1 based on current order ascending
        var ordered = notes.OrderBy(n => n.Order).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Order != i)
            {
                ordered[i].Order = i;
                _noteWriter.Update(ordered[i]);
            }
        }
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notes reordered. Count={Count} UserId={UserId} GroupId={GroupId}", notes.Count, userId, request.NoteGroupId);
    }
}
