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

    public ICollection<Account> Accounts { get; set; } = [];
    public ICollection<Book> Books { get; set; } = [];
    public ICollection<UserLibrary> UserLibraries { get; set; } = [];
    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<BookRating> BookRatings { get; set; } = [];
    public ICollection<InlineComment> InlineComments { get; set; } = [];
    public ICollection<Follow> FollowerFollows { get; set; } = [];   // rows where this user IS the follower
    public ICollection<Follow> FollowingFollows { get; set; } = [];  // rows where this user IS being followed
    
}