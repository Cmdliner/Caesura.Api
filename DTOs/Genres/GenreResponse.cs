namespace Caesura.Api.DTOs.Genres;

public class GenreResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int BookCount { get; set; }
}
