namespace Caesura.Api.Entities;

public record UserLibrary
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public DateTime AddedAt { get; set; }

    public User User { get; set; }
    public Book Book { get; set; }
    
}