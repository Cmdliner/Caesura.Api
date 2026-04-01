namespace Caesura.Api.DTOs.Books;

public class RateBookRequest
{
    public short Rating { get; set; }
    public string? Review { get; set; }
}