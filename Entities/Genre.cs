namespace Caesura.Api.Entities;

public record Genre
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<BookGenre> BookGenres { get; set; } = [];
}