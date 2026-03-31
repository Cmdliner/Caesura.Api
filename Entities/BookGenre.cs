namespace Caesura.Api.Entities;

public record BookGenre
{
    public Guid BookId { get; set; }
    public int GenreId { get; set; }

    public Book Book { get; set; }
    public Genre Genre { get; set; }
}