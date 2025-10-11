using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Notes.SearchNotes;

public sealed class SearchNotesQueryHandler : IRequestHandler<SearchNotesQuery, SearchNotesResponse>
{
    private readonly INoteQueries _noteQueries;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SearchNotesQueryHandler> _logger;

    public SearchNotesQueryHandler(INoteQueries noteQueries, ICurrentUserService currentUser, ILogger<SearchNotesQueryHandler> logger)
    {
        _noteQueries = noteQueries;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<SearchNotesResponse> Handle(SearchNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var result = await _noteQueries.SearchNotesAsync(userId, request.Query, request.PageNumber, request.PageSize, cancellationToken);
        _logger.LogInformation("Notes searched. UserId={UserId} QueryLength={Len} Page={Page} Size={Size} Results={Count}", userId, request.Query?.Length ?? 0, request.PageNumber, request.PageSize, result.Results.Count);
        return result;
    }
}
