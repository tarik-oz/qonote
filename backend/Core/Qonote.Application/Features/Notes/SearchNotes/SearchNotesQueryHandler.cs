using MediatR;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Notes.SearchNotes;

public sealed class SearchNotesQueryHandler : IRequestHandler<SearchNotesQuery, SearchNotesResponse>
{
    private readonly INoteQueries _noteQueries;
    private readonly ICurrentUserService _currentUser;

    public SearchNotesQueryHandler(INoteQueries noteQueries, ICurrentUserService currentUser)
    {
        _noteQueries = noteQueries;
        _currentUser = currentUser;
    }

    public async Task<SearchNotesResponse> Handle(SearchNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        return await _noteQueries.SearchNotesAsync(userId, request.Query, request.PageNumber, request.PageSize, cancellationToken);
    }
}

