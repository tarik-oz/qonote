using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.DeleteNoteGroup;

public sealed record DeleteNoteGroupCommand(
    int Id
) : IRequest, IAuthenticatedRequest;
