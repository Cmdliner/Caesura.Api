using System.Text.Json;

namespace Caesura.Api.DTOs.Books;

public class ChapterDetailResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    // Send ContentHtml if available (fast render), otherwise send Content JSON
    public JsonDocument? Content { get; set; }
    public string? ContentHtml { get; set; }
    public int? WordCount { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class ProgressSummaryResponse
{
    public Guid ChapterId { get; set; }
    public int ScrollPosition { get; set; }
    public DateTime LastReadAt { get; set; }
}