using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.SearchNotes;

public sealed class SearchNotesQuery : IRequest<SearchNotesResponse>, IAuthenticatedRequest
{
    public string Query { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class SearchNotesResponse
{
    public List<SearchResultDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
}


