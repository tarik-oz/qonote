using FluentValidation;
using Qonote.Core.Application.Features.Notes._Shared;

namespace Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;

public sealed class CreateNoteFromYoutubeUrlCommandValidator : AbstractValidator<CreateNoteFromYoutubeUrlCommand>
{
    public CreateNoteFromYoutubeUrlCommandValidator()
    {
        RuleFor(x => x.YoutubeUrl).NotEmpty();
        RuleFor(x => x.YoutubeUrl)
            .Must(url => YouTubeUrlParser.TryExtractVideoId(url) is not null)
            .WithMessage("The provided URL is not a valid YouTube video URL.");
    }
}
