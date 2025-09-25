using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.GetNoteById;

public sealed class GetNoteByIdQuery : IRequest<NoteDto>, IAuthenticatedRequest
{
    public int Id { get; init; }
}
