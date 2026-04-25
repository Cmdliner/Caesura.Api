using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Caesura.Api.Services;

public class BooksService(AppDbContext db)
{
    public async Task<PagedResponse<BookSummaryResponse>> ListBooksAsync(int page = 1, int limit = 20)
    {
        var offset = (page - 1) * limit;
        var total = await db.Books.CountAsync(b => b.Status == "published");

        var items = await db.Books
            .Where(b => b.Status == "published")
            .OrderByDescending(b => b.TotalViews)
            .Skip(offset)
            .Take(limit)
            .Select(b => new BookSummaryResponse
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                Description = b.Description,
                CoverUrl = b.CoverUrl,
                Language = b.Language,
                Status = b.Status,
                TotalViews = b.TotalViews,
                CreatedAt = b.CreatedAt,
                AuthorName = b.Author != null ? b.Author.DisplayName : null,
                Authors = b.BookAuthors.Select(ba => ba.AuthorName).ToList(),
            })
            .ToListAsync();

        return new PagedResponse<BookSummaryResponse>
        {
            Items = items, Page = page, Limit = limit, Total = total
        };
    }

    public async Task<BookDetailResponse?> GetBookBySlugAsync(string slug)
    {
        var book = await db.Books
            .Include(b => b.Author)
            .Include(b => b.BookAuthors)
            .Include(b => b.Chapters
                .Where(c => c.Status == "published")
                .OrderBy(c => c.ChapterNumber))
            .FirstOrDefaultAsync(b => b.Slug == slug && b.Status == "published");

        if (book is null) return null;

        _ = db.Books
            .Where(b => b.Id == book.Id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(b => b.TotalViews, b => b.TotalViews + 1));

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
            GutenbergAuthors = book.BookAuthors.Select(ba => ba.AuthorName).ToList(),
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

    public async Task<Book> CreateBookAsync(Guid authorId, CreateBookRequest req)
    {
        var slug = Slugify(req.Title!) + "-" + Guid.NewGuid().ToString()[..5];
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = req.Title!,
            Slug = slug,
            Description = req.Description,
            CoverUrl = req.CoverUrl,
            Language = req.Language ?? "en",
            Status = "draft",
            Source = "user",
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var authorNames = (req.Authors ?? [])
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Always create at least one BookAuthor row for user-created books.
        if (authorNames.Count == 0)
        {
            var owner = await db.Users
                .Where(u => u.Id == authorId)
                .Select(u => new { u.DisplayName, u.Username })
                .FirstOrDefaultAsync();

            var fallbackAuthorName = owner is null
                ? "Unknown Author"
                : !string.IsNullOrWhiteSpace(owner.DisplayName)
                    ? owner.DisplayName!
                    : owner.Username;

            authorNames.Add(fallbackAuthorName);
        }

        var genreIds = (req.GenreIds ?? [])
            .Distinct()
            .ToList();

        if (genreIds.Count > 0)
        {
            var existingGenreIds = await db.Genres
                .Where(g => genreIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            foreach (var genreId in existingGenreIds)
            {
                book.BookGenres.Add(new BookGenre
                {
                    BookId = book.Id,
                    GenreId = genreId
                });
            }
        }

        foreach (var authorName in authorNames)
        {
            book.BookAuthors.Add(new BookAuthor
            {
                BookId = book.Id,
                AuthorName = authorName
            });
        }

        db.Books.Add(book);
        await db.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateBookAsync(Guid bookId, Guid userId, UpdateBookRequest req)
    {
        var book = await db.Books
            .FirstOrDefaultAsync(b => b.Id == bookId && b.AuthorId == userId);

        if (book is null) return null;

        if (req.Title is not null) book.Title = req.Title;
        if (req.Description is not null) book.Description = req.Description;
        if (req.CoverUrl is not null) book.CoverUrl = req.CoverUrl;
        if (req.Status is not null) book.Status = req.Status;
        book.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return book;
    }

    public async Task<ChapterDetailResponse?> GetChapterAsync(Guid bookId, int chapterNumber)
    {
        var chapter = await db.Chapters
            .FirstOrDefaultAsync(c =>
                c.BookId == bookId &&
                c.ChapterNumber == chapterNumber &&
                c.Status == "published");

        if (chapter is null) return null;

        return new ChapterDetailResponse
        {
            Id = chapter.Id,
            BookId = chapter.BookId,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Content = chapter.ContentHtml is null ? chapter.Content : null,
            ContentHtml = chapter.ContentHtml,
            WordCount = chapter.WordCount,
            PublishedAt = chapter.PublishedAt
        };
    }

    public async Task<Chapter?> CreateChapterAsync(Guid bookId, Guid userId, CreateChapterRequest req)
    {
        var owned = await db.Books
            .AnyAsync(b => b.Id == bookId && b.AuthorId == userId);
        if (!owned) return null;

        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ChapterNumber = req.ChapterNumber,
            Title = req.Title,
            Content = JsonDocument.Parse(req.Content!.Value.GetRawText()),
            ContentHtml = req.ContentHtml,
            WordCount = req.WordCount,
            Status = req.Status ?? "draft",
            PublishedAt = req.Status == "published" ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Chapters.Add(chapter);
        await db.SaveChangesAsync();
        return chapter;
    }

    public async Task<bool> DeleteChapterAsync(Guid bookId, Guid chapterId, Guid userId)
    {
        var owned = await db.Books.AnyAsync(b => b.Id == bookId && b.AuthorId == userId);
        if (!owned) return false;

        var chapter = await db.Chapters
            .FirstOrDefaultAsync(c => c.Id == chapterId && c.BookId == bookId);
        if (chapter is null) return false;

        db.Chapters.Remove(chapter);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Chapter?> UpdateChapterAsync(Guid bookId, Guid chapterId, Guid userId, UpdateChapterRequest req)
    {
        var owned = await db.Books
            .AnyAsync(b => b.Id == bookId && b.AuthorId == userId);
        if (!owned) return null;

        var chapter = await db.Chapters
            .FirstOrDefaultAsync(c => c.Id == chapterId && c.BookId == bookId);
        if (chapter is null) return null;

        if (req.Title is not null) chapter.Title = req.Title;
        if (req.Content.HasValue)
            chapter.Content = JsonDocument.Parse(req.Content.Value.GetRawText());
        if (req.ContentHtml is not null) chapter.ContentHtml = req.ContentHtml;
        if (req.WordCount.HasValue) chapter.WordCount = req.WordCount;
        if (req.Status is not null)
        {
            chapter.Status = req.Status;
            if (req.Status == "published" && chapter.PublishedAt is null)
                chapter.PublishedAt = DateTime.UtcNow;
        }

        chapter.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return chapter;
    }

    public async Task<List<LibraryItemResponse>> GetUserLibraryAsync(Guid userId)
    {
        var library = await db.UserLibraries
            .Where(ul => ul.UserId == userId)
            .Include(ul => ul.Book)
            .OrderByDescending(ul => ul.AddedAt)
            .ToListAsync();

        var bookIds = library.Select(ul => ul.BookId).ToList();
        var progressMap = await db.ReadingProgresses
            .Where(rp => rp.UserId == userId && bookIds.Contains(rp.BookId))
            .ToDictionaryAsync(rp => rp.BookId);

        return library.Select(ul =>
        {
            progressMap.TryGetValue(ul.BookId, out var progress);
            return new LibraryItemResponse
            {
                BookId = ul.BookId,
                AddedAt = ul.AddedAt,
                Title = ul.Book.Title,
                Slug = ul.Book.Slug,
                CoverUrl = ul.Book.CoverUrl,
                TotalViews = ul.Book.TotalViews,
                Progress = progress is null
                    ? null
                    : new ProgressSummaryResponse
                    {
                        ChapterId = progress.ChapterId,
                        ScrollPosition = progress.ScrollPosition,
                        LastReadAt = progress.LastReadAt
                    }
            };
        }).ToList();
    }

    public async Task AddToLibraryAsync(Guid userId, Guid bookId)
    {
        var exists = await db.UserLibraries
            .AnyAsync(ul => ul.UserId == userId && ul.BookId == bookId);
        if (exists) return;

        db.UserLibraries.Add(new UserLibrary
        {
            UserId = userId, BookId = bookId, AddedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task RemoveFromLibraryAsync(Guid userId, Guid bookId)
    {
        await db.UserLibraries
            .Where(ul => ul.UserId == userId && ul.BookId == bookId)
            .ExecuteDeleteAsync();
    }

    public async Task UpsertProgressAsync(Guid userId, Guid bookId, UpdateProgressRequest req)
    {
        var existing = await db.ReadingProgresses
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.BookId == bookId);

        if (existing is null)
        {
            db.ReadingProgresses.Add(new ReadingProgress
            {
                UserId = userId,
                BookId = bookId,
                ChapterId = req.ChapterId,
                ScrollPosition = req.ScrollPosition,
                LastReadAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ChapterId = req.ChapterId;
            existing.ScrollPosition = req.ScrollPosition;
            existing.LastReadAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task<ProgressResponse?> GetProgressAsync(Guid userId, Guid bookId)
    {
        var progress = await db.ReadingProgresses
            .Include(rp => rp.Chapter)
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.BookId == bookId);

        if (progress is null) return null;

        return new ProgressResponse
        {
            BookId = progress.BookId,
            ChapterId = progress.ChapterId,
            ChapterNumber = progress.Chapter.ChapterNumber,
            ScrollPosition = progress.ScrollPosition,
            LastReadAt = progress.LastReadAt
        };
    }

    public async Task<List<InlineCommentResponse>> GetInlineCommentsAsync(Guid chapterId)
    {
        return await db.InlineComments
            .Where(ic => ic.ChapterId == chapterId)
            .Include(ic => ic.User)
            .OrderBy(ic => ic.FromPos)
            .Select(ic => new InlineCommentResponse
            {
                Id = ic.Id,
                ChapterId = ic.ChapterId,
                FromPos = ic.FromPos,
                ToPos = ic.ToPos,
                QuoteText = ic.QuoteText,
                Content = ic.Content,
                CreatedAt = ic.CreatedAt,
                Author = new CommentAuthorResponse
                {
                    Id = ic.User.Id,
                    Username = ic.User.Username,
                    AvatarUrl = ic.User.AvatarUrl
                }
            })
            .ToListAsync();
    }

    public async Task<InlineCommentResponse> CreateInlineCommentAsync(Guid chapterId, Guid userId, InlineCommentRequest req)
    {
        var comment = new InlineComment
        {
            Id = Guid.NewGuid(),
            ChapterId = chapterId,
            UserId = userId,
            FromPos = req.FromPos,
            ToPos = req.ToPos,
            QuoteText = req.QuoteText!,
            Content = req.Content!,
            CreatedAt = DateTime.UtcNow
        };
        db.InlineComments.Add(comment);
        await db.SaveChangesAsync();

        var user = await db.Users.FindAsync(userId);
        return new InlineCommentResponse
        {
            Id = comment.Id,
            ChapterId = comment.ChapterId,
            FromPos = comment.FromPos,
            ToPos = comment.ToPos,
            QuoteText = comment.QuoteText,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            Author = new CommentAuthorResponse
            {
                Id = user!.Id,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl
            }
        };
    }

    public async Task<RatingResponse> RateBookAsync(Guid userId, Guid bookId, RateBookRequest req)
    {
        var existing = await db.BookRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);

        if (existing is null)
        {
            db.BookRatings.Add(new BookRating
            {
                UserId = userId,
                BookId = bookId,
                Rating = req.Rating,
                Review = req.Review,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Rating = req.Rating;
            existing.Review = req.Review;
        }

        await db.SaveChangesAsync();

        var stats = await db.BookRatings
            .Where(r => r.BookId == bookId)
            .GroupBy(r => r.BookId)
            .Select(g => new { Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .FirstAsync();

        return new RatingResponse
        {
            Success = true,
            AverageRating = Math.Round(stats.Avg, 1),
            TotalRatings = stats.Count
        };
    }

    private static string Slugify(string text) =>
        new string(text.ToLower()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray())
            .Trim('-')[..Math.Min(80, text.Length)];
    
}