namespace Caesura.Api.Entities;

public record User
{
    public Guid Id { get; init; }

    public required string Email { get; set; }

    public required string Username { get; set; }

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<User> Accounts { get; set; } = [];
}