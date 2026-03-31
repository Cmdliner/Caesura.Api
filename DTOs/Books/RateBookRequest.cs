namespace Caesura.Api.DTOs.Books;

public class RateBookRequest
{
    public short Rating { get; set; }       // 1–5
    public string? Review { get; set; }
}