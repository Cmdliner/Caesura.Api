namespace Caesura.Api.DTOs.Books;

public class CreateBookRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string Language { get; set; } = "en";
}