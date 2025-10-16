using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<CreateNoteFromYoutubeUrlCommandHandler> _logger;

    public CreateNoteFromYoutubeUrlCommandHandler(
        IWriteRepository<Note, int> noteWriteRepository,
        IReadRepository<Note, int> noteReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IYouTubeMetadataService youTube,
        IImageService imageService,
        INoteFactory noteFactory,
        ILimitCheckerService limitChecker,
        ILogger<CreateNoteFromYoutubeUrlCommandHandler> logger)
    {
        _noteWriteRepository = noteWriteRepository;
        _noteReadRepository = noteReadRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _youTube = youTube;
        _imageService = imageService;
        _noteFactory = noteFactory;
        _limitChecker = limitChecker;
        _logger = logger;
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

        // Enforce subscription limit for creating notes and consume one quota
        await _limitChecker.EnsureAndConsumeNoteQuotaAsync(userId, cancellationToken);

        // Per-user title uniqueness and duplicate video are enforced via business rules (pipeline behavior)
        var trimmedCustomTitle = request.CustomTitle?.Trim();

        // If a rule already fetched metadata, reuse it to avoid duplicate call
        var meta = request.Metadata ?? await _youTube.GetVideoMetadataAsync(videoId, cancellationToken);

        // Create entity via factory (business rules already validate title uniqueness incl. derived)
        var note = _noteFactory.CreateFromYouTubeMetadata(meta, userId, request.YoutubeUrl, trimmedCustomTitle);

        // Set Order: put new note at the top of ungrouped list without shifting others
        var existingNotes = await _noteReadRepository.GetAllAsync(
            n => n.UserId == userId && n.NoteGroupId == null,
            cancellationToken);
        if (existingNotes.Any())
        {
            var currentMin = existingNotes.Min(n => n.Order);
            // Guard against int underflow in pathological cases
            note.Order = currentMin <= int.MinValue + 1 ? int.MinValue + 1 : currentMin - 1;
        }
        else
        {
            note.Order = 0;
        }

        await _noteWriteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Note created from YouTube. NoteId={NoteId} UserId={UserId} VideoId={VideoId} HasCustomTitle={HasCustomTitle}", note.Id, userId, videoId, trimmedCustomTitle is not null);

        // Best-effort: upload thumbnail to blob and update note url
        if (!string.IsNullOrWhiteSpace(meta.ThumbnailUrl))
        {
            var uploadedUrl = await _imageService.UploadNoteThumbnailFromUrlAsync(meta.ThumbnailUrl, userId, note.Id, cancellationToken);
            if (!string.IsNullOrWhiteSpace(uploadedUrl))
            {
                note.ThumbnailUrl = uploadedUrl!;
                _noteWriteRepository.Update(note);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Note thumbnail uploaded. NoteId={NoteId} UserId={UserId}", note.Id, userId);
            }
            else
            {
                // fallback: keep original meta url
                note.ThumbnailUrl = meta.ThumbnailUrl;
                _logger.LogWarning("Note thumbnail upload failed, kept metadata URL. NoteId={NoteId} UserId={UserId}", note.Id, userId);
            }
        }

        return note.Id;
    }
}
