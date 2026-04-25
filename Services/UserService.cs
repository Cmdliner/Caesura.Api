using Microsoft.EntityFrameworkCore;

namespace Caesura.Api.Services;

public interface IUserService
{
    public Task<IEnumerable<AuthoredBooksResponse>> GetAuthoredBooks(Guid userId);
    public Task<BookDetailResponse?> GetBookBySlugAsync(string slug, Guid userId);
    public Task<ChapterDetailResponse?> GetChapterForEditAsync(Guid bookId, Guid userId, int chapterNumber);
}

public class UserService(AppDbContext db) : IUserService
{
    public async Task<IEnumerable<AuthoredBooksResponse>> GetAuthoredBooks(Guid userId)
    {
        return await db.Books
            .Where(b => b.AuthorId == userId)
            .Select(b => new AuthoredBooksResponse
            {
                Id = b.Id,
                Slug = b.Slug,
                Description = b.Description,
                Title = b.Title,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                CoverUrl = b.CoverUrl,
                ChapterCount = b.Chapters.Count(),
                TotalWordCount = b.Chapters.Sum(c => c.WordCount ?? 0),
            })
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync();
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

    public async Task<ChapterDetailResponse?> GetChapterForEditAsync(Guid bookId, Guid userId, int chapterNumber)
    {
        var chapter = await db.Chapters
            .FirstOrDefaultAsync(c =>
                c.BookId == bookId &&
                c.ChapterNumber == chapterNumber &&
                c.Book.AuthorId == userId);

        if (chapter is null) return null;

        return new ChapterDetailResponse
        {
            Id = chapter.Id,
            BookId = chapter.BookId,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Content = chapter.Content,
            ContentHtml = chapter.ContentHtml,
            WordCount = chapter.WordCount,
            PublishedAt = chapter.PublishedAt
        };
    }
}