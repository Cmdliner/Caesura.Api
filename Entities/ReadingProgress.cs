namespace Caesura.Api.Entities;

public record ReadingProgress
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChapterId { get; set; }
    public int ScrollPosition { get; set; } //Scroll position as int percentage
    public DateTime lastReadAt { get; set; }

    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public Chapter Chapter { get; set; } = null!;
}