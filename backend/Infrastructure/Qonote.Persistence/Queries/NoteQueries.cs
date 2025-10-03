using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Core.Application.Features.Notes.SearchNotes;

namespace Qonote.Infrastructure.Persistence.Queries;

public sealed class NoteQueries : INoteQueries
{
    private readonly ApplicationDbContext _db;
    private readonly IConfigurationProvider _mapperConfig;

    public NoteQueries(ApplicationDbContext db, IConfigurationProvider mapperConfig)
    {
        _db = db;
        _mapperConfig = mapperConfig;
    }

    public async Task<List<NoteListItemDto>> GetOrderedNotesForGroupAsync(int groupId, string userId, int? offset, int? limit, CancellationToken cancellationToken)
    {
        var query = _db.Set<Note>()
            .AsNoTracking()
            .Where(n => !n.IsDeleted && n.UserId == userId && n.NoteGroupId == groupId)
            .OrderBy(n => n.Order)
            .AsQueryable();

        if (offset is int o && o > 0)
        {
            query = query.Skip(o);
        }
        if (limit is int l && l > 0)
        {
            query = query.Take(l);
        }

        return await query
            .ProjectTo<NoteListItemDto>(_mapperConfig)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NoteListItemDto>> GetOrderedUngroupedNotesAsync(string userId, int? offset, int? limit, CancellationToken cancellationToken)
    {
        var query = _db.Set<Note>()
            .AsNoTracking()
            .Where(n => !n.IsDeleted && n.UserId == userId && n.NoteGroupId == null)
            .OrderBy(n => n.Order)
            .AsQueryable();

        if (offset is int o && o > 0)
        {
            query = query.Skip(o);
        }
        if (limit is int l && l > 0)
        {
            query = query.Take(l);
        }

        return await query
            .ProjectTo<NoteListItemDto>(_mapperConfig)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveNotesAsync(string userId, CancellationToken cancellationToken)
    {
        return await _db.Set<Note>()
            .AsNoTracking()
            .Where(n => !n.IsDeleted && n.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<SearchNotesResponse> SearchNotesAsync(string userId, string query, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        // Sanitize query: remove special characters and convert to tsquery format
        var sanitizedQuery = SanitizeSearchQuery(query);
        
        if (string.IsNullOrWhiteSpace(sanitizedQuery))
        {
            return new SearchNotesResponse 
            { 
                Results = new List<SearchResultDto>(), 
                TotalCount = 0 
            };
        }

        // Build tsquery (AND operator between words)
        var tsQuery = string.Join(" & ", sanitizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // PostgreSQL FTS query with ranking and highlighting
        var sql = @"
            WITH search_results AS (
                SELECT 
                    n.""Id"" AS note_id,
                    COALESCE(n.""CustomTitle"", n.""VideoTitle"") AS title,
                    n.""ThumbnailUrl"" AS thumbnail_url,
                    ts_rank(n.""SearchVector"", to_tsquery('english', {0})) AS rank,
                    ts_headline('english', COALESCE(n.""CustomTitle"", n.""VideoTitle""), to_tsquery('english', {0}), 
                        'StartSel=<mark>, StopSel=</mark>, MaxWords=50, MinWords=10') AS highlighted_title
                FROM ""Notes"" n
                WHERE n.""UserId"" = {1}
                  AND n.""IsDeleted"" = false
                  AND n.""SearchVector"" @@ to_tsquery('english', {0})
                ORDER BY rank DESC, n.""UpdatedAt"" DESC
                LIMIT {2} OFFSET {3}
            )
            SELECT * FROM search_results;
        ";

        var skip = (pageNumber - 1) * pageSize;
        
        var results = await _db.Database
            .SqlQueryRaw<SearchResultRaw>(sql, tsQuery, userId, pageSize, skip)
            .ToListAsync(cancellationToken);

        // Get total count
        var countSql = @"
            SELECT COUNT(*) AS count
            FROM ""Notes"" n
            WHERE n.""UserId"" = {0}
              AND n.""IsDeleted"" = false
              AND n.""SearchVector"" @@ to_tsquery('english', {1})
        ";
        
        var countResult = await _db.Database
            .SqlQueryRaw<CountResult>(countSql, userId, tsQuery)
            .FirstOrDefaultAsync(cancellationToken);
        
        var totalCount = countResult?.count ?? 0;

        // Get snippets for each note (from Sections/Blocks)
        var noteIds = results.Select(r => r.note_id).ToList();
        var snippets = await GetSnippetsForNotes(noteIds, tsQuery, cancellationToken);

        var searchResults = results.Select(r => new SearchResultDto
        {
            NoteId = r.note_id,
            Title = r.title,
            HighlightedTitle = r.highlighted_title,
            ThumbnailUrl = r.thumbnail_url,
            Rank = r.rank,
            Snippets = snippets.ContainsKey(r.note_id) ? snippets[r.note_id] : new List<SnippetDto>()
        }).ToList();

        return new SearchNotesResponse
        {
            Results = searchResults,
            TotalCount = totalCount
        };
    }

    private async Task<Dictionary<int, List<SnippetDto>>> GetSnippetsForNotes(
        List<int> noteIds, 
        string tsQuery, 
        CancellationToken cancellationToken)
    {
        if (!noteIds.Any())
        {
            return new Dictionary<int, List<SnippetDto>>();
        }

        // Get snippets from Section titles and Block content
        var sql = @"
            SELECT 
                s.""NoteId"" AS note_id,
                ts_headline('english', s.""Title"", to_tsquery('english', {0}), 
                    'StartSel=<mark>, StopSel=</mark>, MaxWords=10, MinWords=1') AS section_title,
                ts_headline('english', COALESCE(b.""Content"", s.""Title""), to_tsquery('english', {0}), 
                    'StartSel=<mark>, StopSel=</mark>, MaxWords=30, MinWords=10') AS content
            FROM ""Sections"" s
            LEFT JOIN ""Blocks"" b ON b.""SectionId"" = s.""Id"" AND b.""IsDeleted"" = false
            WHERE s.""NoteId"" = ANY({1})
              AND s.""IsDeleted"" = false
              AND (
                  to_tsvector('english', s.""Title"") @@ to_tsquery('english', {0})
                  OR to_tsvector('english', COALESCE(b.""Content"", '')) @@ to_tsquery('english', {0})
              )
            LIMIT 5;
        ";

        var snippetResults = await _db.Database
            .SqlQueryRaw<SnippetRaw>(sql, tsQuery, noteIds)
            .ToListAsync(cancellationToken);

        return snippetResults
            .GroupBy(s => s.note_id)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => new SnippetDto
                {
                    SectionTitle = s.section_title,
                    Content = s.content
                }).ToList()
            );
    }

    private string SanitizeSearchQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        // Remove special PostgreSQL FTS characters to prevent injection
        var sanitized = query
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("|", "")
            .Replace("&", "")
            .Replace("!", "")
            .Replace("<", "")
            .Replace(">", "")
            .Trim();

        return sanitized;
    }
}

// Raw SQL result types (snake_case from PostgreSQL)
public class SearchResultRaw
{
    public int note_id { get; set; }
    public string title { get; set; } = string.Empty;
    public string thumbnail_url { get; set; } = string.Empty;
    public float rank { get; set; }
    public string highlighted_title { get; set; } = string.Empty;
}

public class SnippetRaw
{
    public int note_id { get; set; }
    public string section_title { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
}

public class CountResult
{
    public int count { get; set; }
}
