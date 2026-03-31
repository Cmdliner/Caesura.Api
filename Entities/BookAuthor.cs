namespace Caesura.Api.Entities;

public record BookAuthor
{
    public Guid BookId { get; set; }
    public required string AuthorName { get; set; }

    public Book Book { get; set; } = null!;
}