using MediatR;
using AutoMapper;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.YouTube;
using Qonote.Core.Application.Features.Notes._Shared;
using Qonote.Core.Application.Abstractions.Storage;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;

public sealed class CreateNoteFromYoutubeUrlCommandHandler : IRequestHandler<CreateNoteFromYoutubeUrlCommand, int>
{
    private readonly IReadRepository<Note, int> _noteReadRepository;
    private readonly IWriteRepository<Note, int> _noteWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IYouTubeMetadataService _youTube;
    private readonly IFileStorageService _fileStorageService;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly INoteFactory _noteFactory;
    private const string ThumbnailsContainer = "thumbnails";

    public CreateNoteFromYoutubeUrlCommandHandler(
        IReadRepository<Note, int> noteReadRepository,
        IWriteRepository<Note, int> noteWriteRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IYouTubeMetadataService youTube,
        IFileStorageService fileStorageService,
        HttpClient httpClient,
        IMapper mapper,
        INoteFactory noteFactory)
    {
        _noteReadRepository = noteReadRepository;
        _noteWriteRepository = noteWriteRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _youTube = youTube;
        _fileStorageService = fileStorageService;
        _httpClient = httpClient;
        _mapper = mapper;
        _noteFactory = noteFactory;
    }

    public async Task<int> Handle(CreateNoteFromYoutubeUrlCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var videoId = YouTubeUrlParser.TryExtractVideoId(request.YoutubeUrl);
        if (videoId is null)
        {
            // This should be caught by validator, defensive check here
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("YoutubeUrl", "Invalid YouTube URL.") });
        }

        // Per-user title uniqueness and duplicate video are enforced via business rules (pipeline behavior)
        var trimmedCustomTitle = request.CustomTitle?.Trim();

        var meta = await _youTube.GetVideoMetadataAsync(videoId, cancellationToken);

        // Map core fields from YouTube metadata using AutoMapper, then fill remaining fields
        var note = _mapper.Map<Note>(meta);
        note.UserId = userId;
        note.YoutubeUrl = request.YoutubeUrl;
        note.CustomTitle = string.IsNullOrWhiteSpace(request.CustomTitle) ? meta.Title : request.CustomTitle;
        note.Sections = new List<Section>();

        // If CustomTitle was not provided, we derived it from video title; ensure uniqueness per-user now
        if (string.IsNullOrWhiteSpace(trimmedCustomTitle))
        {
            var derivedTitle = note.CustomTitle.Trim();
            var conflictOnDerived = await _noteReadRepository.GetAllAsync(
                n => n.UserId == userId && n.CustomTitle.ToLower() == derivedTitle.ToLower(),
                cancellationToken);
            if (conflictOnDerived.Any())
            {
                var exId = conflictOnDerived.First().Id;
                throw new ConflictException(
                    "You already have a note with this title.",
                    new Dictionary<string, string[]>
                    {
                        { "field", new[] { "CustomTitle" } },
                        { "existingNoteId", new[] { exId.ToString() } }
                    });
            }
        }

        note = _noteFactory.CreateFromYouTubeMetadata(meta, userId, request.YoutubeUrl, request.CustomTitle);

        await _noteWriteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort: upload thumbnail to blob and update note url
        if (!string.IsNullOrWhiteSpace(meta.ThumbnailUrl))
        {
            try
            {
                var response = await _httpClient.GetAsync(meta.ThumbnailUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                    var fileName = $"note_{note.Id}.jpg";
                    var blobUrl = await _fileStorageService.UploadAsync(stream, ThumbnailsContainer, fileName, contentType);
                    note.ThumbnailUrl = blobUrl;
                    _noteWriteRepository.Update(note);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    // fallback: keep original meta url
                    note.ThumbnailUrl = meta.ThumbnailUrl;
                }
            }
            catch
            {
                // ignore errors; fallback to meta url if not set yet
                if (string.IsNullOrWhiteSpace(note.ThumbnailUrl))
                {
                    note.ThumbnailUrl = meta.ThumbnailUrl;
                    _noteWriteRepository.Update(note);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        return note.Id;
    }
}
