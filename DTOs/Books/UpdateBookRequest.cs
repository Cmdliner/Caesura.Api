namespace Caesura.Api.DTOs.Books;

public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string? Status { get; set; }     // draft | published | completed
    public List<int>? GenreIds { get; set; }
}