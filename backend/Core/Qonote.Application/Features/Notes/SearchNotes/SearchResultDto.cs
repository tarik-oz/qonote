namespace Qonote.Core.Application.Features.Notes.SearchNotes;

public sealed class SearchResultDto
{
    public int NoteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HighlightedTitle { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public float Rank { get; set; }
    public List<SnippetDto> Snippets { get; set; } = new();
}

public sealed class SnippetDto
{
    public string SectionTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}


