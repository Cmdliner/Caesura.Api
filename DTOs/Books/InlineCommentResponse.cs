namespace Caesura.Api.DTOs.Books;

public class InlineCommentResponse
{
    public Guid Id { get; set; }
    public Guid ChapterId { get; set; }
    public int FromPos { get; set; }
    public int ToPos { get; set; }
    public string QuoteText { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public CommentAuthorResponse Author { get; set; } = null!;
}

public class CommentAuthorResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}