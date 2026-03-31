namespace Caesura.Api.Entities;

public record InlineComment
{
    public Guid Id { get; set; }
    public Guid ChapterId { get; set; }
    public Guid UserId { get; set; }
    public int FromPos { get; set; }            // ProseMirror offset start
    public int ToPos { get; set; }              // ProseMirror offset end
    public string QuoteText { get; set; } = null!;   // snapshot of highlighted text
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    public Chapter Chapter { get; set; } = null!;
    public User User { get; set; } = null!;
}