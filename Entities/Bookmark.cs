namespace Caesura.Api.Entities;

public record Bookmark
{
    public long Id { get; set; }
    public Guid ChapterId { get; set; }
    public Guid UserId { get; set; }
    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Chapter Chapter { get; set; } = null!;
}