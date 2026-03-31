namespace Caesura.Api.Entities;

public record BookRating
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public short Rating { get; set; }           // 1–5
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
 
    // Navigation
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
}