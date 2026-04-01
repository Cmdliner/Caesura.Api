using System.Text.Json;

namespace Caesura.Api.DTOs.Books;

public class CreateChapterRequest
{
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    public JsonElement? Content { get; set; }
    public string? ContentHtml { get; set; }
    public int? WordCount { get; set; }
    public string Status { get; set; } = "draft";
}