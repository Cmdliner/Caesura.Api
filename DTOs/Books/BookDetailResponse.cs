namespace Caesura.Api.DTOs.Books;

public class BookDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string Language { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Source { get; set; } = null!;
    public int TotalViews { get; set; }
    public DateTime CreatedAt { get; set; }
    public AuthorResponse? Author { get; set; }         // null for Gutenberg books
    public List<string> GutenbergAuthors { get; set; } = [];  // for Gutenberg books
    public List<ChapterSummaryResponse> Chapters { get; set; } = [];
    public List<string> Genres { get; set; } = [];
}

public class AuthorResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}
public class ChapterSummaryResponse
{
    public Guid Id { get; set; }
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    public int? WordCount { get; set; }
    public DateTime? PublishedAt { get; set; }
}