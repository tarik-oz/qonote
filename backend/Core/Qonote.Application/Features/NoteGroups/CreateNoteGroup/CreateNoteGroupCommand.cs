using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;

public sealed record CreateNoteGroupCommand(
    string Title
) : IRequest<int>, IAuthenticatedRequest;
