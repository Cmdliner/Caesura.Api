namespace Caesura.Api.DTOs.Books;

public class BookSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? AuthorName { get; set; }
    public ICollection<string> Authors { get; set; } = Array.Empty<string>();
    public string? CoverUrl { get; set; }
    public string Language { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int TotalViews { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Genres { get; set; } = [];
}