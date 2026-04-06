using Microsoft.EntityFrameworkCore;

namespace Caesura.Api.Services;

public interface IUserService
{
    public Task<IEnumerable<AuthoredBooksResponse>> GetAuthoredBooks(Guid userId);
    public Task<BookDetailResponse?> GetBookBySlugAsync(string slug, Guid userId);
}

public class UserService(AppDbContext db) : IUserService
{
    public async Task<IEnumerable<AuthoredBooksResponse>> GetAuthoredBooks(Guid userId)
    {
        var authoredBooks = await db.BookAuthors
            .Where(ba => ba.Book.AuthorId == userId)
            .Select(ba => new AuthoredBooksResponse
            {
                Id = ba.Book.Id,
                Slug = ba.Book.Slug,
                Description = ba.Book.Description,
                Title = ba.Book.Title,
                Status = ba.Book.Status,
                CreatedAt = ba.Book.CreatedAt,
                CoverUrl = ba.Book.CoverUrl,
            })
            .ToListAsync();
        return authoredBooks;
    }

    public async Task<BookDetailResponse?> GetBookBySlugAsync(string slug, Guid userId)
    {
        var book = await db.Books
            .Include(b => b.Author)
            .Include(b => b.BookAuthors)
            .Include(b => b.Chapters.OrderBy(c => c.ChapterNumber))
            .FirstOrDefaultAsync(b => b.Slug == slug && b.AuthorId == userId);

        if (book is null) return null;

        return new BookDetailResponse
        {
            Id = book.Id,
            Title = book.Title,
            Slug = book.Slug,
            Description = book.Description,
            CoverUrl = book.CoverUrl,
            Language = book.Language,
            Status = book.Status,
            Source = book.Source,
            TotalViews = book.TotalViews,
            CreatedAt = book.CreatedAt,
            Author = book.Author is null
                ? null
                : new AuthorResponse
                {
                    Id = book.Author.Id,
                    Username = book.Author.Username,
                    DisplayName = book.Author.DisplayName,
                    AvatarUrl = book.Author.AvatarUrl
                },
            Chapters = book.Chapters.Select(c => new ChapterSummaryResponse
            {
                Id = c.Id,
                ChapterNumber = c.ChapterNumber,
                Title = c.Title,
                WordCount = c.WordCount,
                PublishedAt = c.PublishedAt
            }).ToList()
        };
    }
}