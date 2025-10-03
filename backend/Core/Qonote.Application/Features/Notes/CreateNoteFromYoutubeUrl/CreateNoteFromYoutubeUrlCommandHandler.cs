using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.YouTube;
using Qonote.Core.Application.Features.Notes._Shared;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Abstractions.Media;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;

public sealed class CreateNoteFromYoutubeUrlCommandHandler : IRequestHandler<CreateNoteFromYoutubeUrlCommand, int>
{
    private readonly IWriteRepository<Note, int> _noteWriteRepository;
    private readonly IReadRepository<Note, int> _noteReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IYouTubeMetadataService _youTube;
    private readonly IImageService _imageService;
    private readonly INoteFactory _noteFactory;
    private readonly ILimitCheckerService _limitChecker;

    public CreateNoteFromYoutubeUrlCommandHandler(
        IWriteRepository<Note, int> noteWriteRepository,
        IReadRepository<Note, int> noteReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IYouTubeMetadataService youTube,
        IImageService imageService,
        INoteFactory noteFactory,
        ILimitCheckerService limitChecker)
    {
        _noteWriteRepository = noteWriteRepository;
        _noteReadRepository = noteReadRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _youTube = youTube;
        _imageService = imageService;
        _noteFactory = noteFactory;
        _limitChecker = limitChecker;
    }

    public async Task<int> Handle(CreateNoteFromYoutubeUrlCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        var videoId = YouTubeUrlParser.TryExtractVideoId(request.YoutubeUrl);
        if (videoId is null)
        {
            // This should be caught by validator, defensive check here
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("YoutubeUrl", "Invalid YouTube URL.") });
        }

        // Enforce subscription limit for creating notes
        await _limitChecker.EnsureUserCanCreateNoteAsync(userId, cancellationToken);

        // Per-user title uniqueness and duplicate video are enforced via business rules (pipeline behavior)
        var trimmedCustomTitle = request.CustomTitle?.Trim();

        // If a rule already fetched metadata, reuse it to avoid duplicate call
        var meta = request.Metadata ?? await _youTube.GetVideoMetadataAsync(videoId, cancellationToken);

        // Create entity via factory (business rules already validate title uniqueness incl. derived)
        var note = _noteFactory.CreateFromYouTubeMetadata(meta, userId, request.YoutubeUrl, trimmedCustomTitle);

        // Set Order: new notes go to the end
        var existingNotes = await _noteReadRepository.GetAllAsync(
            n => n.UserId == userId && n.NoteGroupId == null, 
            cancellationToken);
        note.Order = existingNotes.Any() ? existingNotes.Max(n => n.Order) + 1 : 0;

        await _noteWriteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort: upload thumbnail to blob and update note url
        if (!string.IsNullOrWhiteSpace(meta.ThumbnailUrl))
        {
            var uploadedUrl = await _imageService.UploadNoteThumbnailFromUrlAsync(meta.ThumbnailUrl, userId, note.Id, cancellationToken);
            if (!string.IsNullOrWhiteSpace(uploadedUrl))
            {
                note.ThumbnailUrl = uploadedUrl!;
                _noteWriteRepository.Update(note);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // fallback: keep original meta url
                note.ThumbnailUrl = meta.ThumbnailUrl;
            }
        }

        return note.Id;
    }
}
