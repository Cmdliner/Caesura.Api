namespace Caesura.Api.DTOs.Books;

public class RatingResponse
{
    public bool Success { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
}