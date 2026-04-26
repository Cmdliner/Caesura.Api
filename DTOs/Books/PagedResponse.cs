namespace Caesura.Api.DTOs.Books;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)Total / Limit) : 0;
    public bool HasMore => Page < TotalPages;
}