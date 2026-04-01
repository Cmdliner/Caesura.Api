namespace Caesura.Api.DTOs.Books;

public class InlineCommentRequest
{
    public int FromPos { get; set; }
    public int ToPos { get; set; }
    public string? QuoteText { get; set; }
    public string? Content { get; set; }
}