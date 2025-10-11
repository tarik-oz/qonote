using FluentValidation;
using Qonote.Core.Application.Extensions;
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

        RuleFor(x => x.CustomTitle ?? string.Empty)
            .TrimmedMaxLength(200, "Title must not exceed 200 characters.")
            .TrimmedMatches(@"^[\p{L}\p{N}\p{P}\p{S} ]+$", "Title can only contain letters (including Turkish), numbers, punctuation, symbols, and spaces.")
            .When(x => x.CustomTitle is not null);
    }
}
