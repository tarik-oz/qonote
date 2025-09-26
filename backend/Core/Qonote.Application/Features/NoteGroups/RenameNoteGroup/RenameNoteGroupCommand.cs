using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.RenameNoteGroup;

public sealed record RenameNoteGroupCommand(
    int Id,
    string Title
) : IRequest, IAuthenticatedRequest;
