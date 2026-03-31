namespace Caesura.Api.Entities;

public record Book
{
    public Guid Id { get; set; }
    public int? GutenbergId { get; set; }

    public required string Title { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string Language { get; set; } = "en";
    public string Status { get; set; } = "draft"; // "draft||published||completed"
    public string Source { get; set; } = "user"; // "gutenberg|user"
    public Guid? AuthorId { get; set; }
    public int TotalViews { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    public User? Author { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = [];
    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
    public ICollection<BookGenre> BookGenres { get; set; } = [];
    public ICollection<BookRating> BookRatings { get; set; } = [];
    public ICollection<UserLibrary> UserLibraries  { get; set; } = [];
    
}