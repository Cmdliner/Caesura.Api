namespace Caesura.Api.DTOs.User;

public class AuthoredBooksResponse
{
    public required string Title { get; set; }
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public required string Status { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}