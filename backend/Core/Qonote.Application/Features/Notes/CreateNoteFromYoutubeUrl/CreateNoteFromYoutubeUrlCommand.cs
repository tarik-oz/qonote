using MediatR;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Application.Abstractions.YouTube.Models;

namespace Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;

public sealed class CreateNoteFromYoutubeUrlCommand : IRequest<int>, IAuthenticatedRequest
{
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? CustomTitle { get; set; }

    // Carries fetched metadata (populated by a rule) to avoid duplicate external calls in handler
    internal YouTubeVideoMetadata? Metadata { get; set; }
}
