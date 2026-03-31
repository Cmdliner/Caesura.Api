namespace Caesura.Api.DTOs.Books;

public class UpdateProgressRequest
{
    public Guid ChapterId { get; set; }
    public int ScrollPosition { get; set; }     // 0–10000 (percentage × 100)
}