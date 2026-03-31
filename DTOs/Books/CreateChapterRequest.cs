using System.Text.Json;

namespace Caesura.Api.DTOs.Books;

public class CreateChapterRequest
{
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    // ProseMirror JSON document from TipTap editor
    // Must be { "type": "doc", "content": [...] }
    public JsonElement Content { get; set; }
    // HTML pre-rendered by TipTap on the client before sending
    // Stored as a cache for fast SSR — not the source of truth
    public string? ContentHtml { get; set; }
    public int? WordCount { get; set; }
    public string Status { get; set; } = "draft";
}