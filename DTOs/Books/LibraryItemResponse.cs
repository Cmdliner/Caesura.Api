namespace Caesura.Api.DTOs.Books;

public class LibraryItemResponse
{    public Guid BookId { get; set; }
    public DateTime AddedAt { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? CoverUrl { get; set; }
    public int TotalViews { get; set; }
    // Current reading position for this book (null if not started)
    public ProgressSummaryResponse? Progress { get; set; }

    
}