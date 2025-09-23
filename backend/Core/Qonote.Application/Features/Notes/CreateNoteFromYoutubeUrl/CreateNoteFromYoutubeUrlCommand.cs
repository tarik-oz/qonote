using MediatR;

namespace Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;

public sealed class CreateNoteFromYoutubeUrlCommand : IRequest<int>
{
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? CustomTitle { get; set; }
}
