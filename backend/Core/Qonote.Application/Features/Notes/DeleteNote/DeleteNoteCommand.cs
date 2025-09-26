using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.DeleteNote;

public sealed record DeleteNoteCommand(
    int Id
) : IRequest, IAuthenticatedRequest;
