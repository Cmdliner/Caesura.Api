namespace Caesura.Api.Entities;

public record Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Provider { get; set; } // "email" || "google"
    public string? ProviderUserId { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

}