using System.Diagnostics.Eventing.Reader;
using System.Text.Json;

namespace Caesura.Api.Entities;

public class Chapter
{
    public Guid Id { get; init; }
    public Guid BookId { get; set; }
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    // ProseMirror JSON - stored as jsonb in postgres db. EF maps this as a string
    // so there's a need to deserialize in the service layer
    public JsonDocument Content { get; set; } = null!;
    public string? ContentHtml { get; set; } // Cached HTML that is generated at publish time
    public int? WordCount { get; set; }
    public string Status { get; set; } = "draft"; // "published"| draft
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    public Book Book { get; set; } = null!;
    public ICollection<InlineComment> InlineComments { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<ReadingProgress> ReadingProgress { get; set; } = [];

}