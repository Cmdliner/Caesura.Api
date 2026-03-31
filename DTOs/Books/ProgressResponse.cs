namespace Caesura.Api.DTOs.Books;

public class ProgressResponse
{
    public Guid BookId { get; set; }
    public Guid ChapterId { get; set; }
    public int ChapterNumber { get; set; }
    public int ScrollPosition { get; set; }
    public DateTime LastReadAt { get; set; }
}