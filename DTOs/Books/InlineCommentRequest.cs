namespace Caesura.Api.DTOs.Books;

public class InlineCommentRequest
{
    public int FromPos { get; set; }        // ProseMirror character offset start
    public int ToPos { get; set; }          // ProseMirror character offset end
    public string QuoteText { get; set; } = null!;  // snapshot of highlighted text
    public string Content { get; set; } = null!;
}