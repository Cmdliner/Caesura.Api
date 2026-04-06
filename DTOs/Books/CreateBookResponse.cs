namespace Caesura.Api.DTOs.Books;

public class CreateBookResponse
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
}

