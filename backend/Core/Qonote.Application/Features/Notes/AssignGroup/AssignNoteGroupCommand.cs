using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.AssignGroup;

public sealed record AssignNoteGroupCommand(
    int NoteId,
    int? NoteGroupId
) : IRequest, IAuthenticatedRequest;
