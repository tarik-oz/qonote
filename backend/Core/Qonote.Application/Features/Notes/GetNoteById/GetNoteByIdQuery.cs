using MediatR;

namespace Qonote.Core.Application.Features.Notes.GetNoteById;

public sealed class GetNoteByIdQuery : IRequest<NoteDto>
{
    public int Id { get; init; }
}
