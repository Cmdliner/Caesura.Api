using Microsoft.EntityFrameworkCore;

namespace Caesura.Api.Services;

/// <summary>
/// Handles genre listing and genre-filtered book browsing.
/// </summary>
public class GenresService(AppDbContext db)
{
    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns every genre ordered by name, with its published-book count.</summary>
    public async Task<List<GenreResponse>> GetAllGenresAsync()
    {
        var genres = await db.Genres
            .OrderBy(g => g.Name)
            .Select(g => new
            {
                g.Id,
                g.Name,
                BookCount = g.BookGenres.Count(bg => bg.Book.Status == "published"),
            })
            .ToListAsync();

        return genres
            .Select(g => new GenreResponse
            {
                Id = g.Id,
                Name = g.Name,
                Slug = ToSlug(g.Name),
                BookCount = g.BookCount,
            })
            .ToList();
    }

    /// <summary>
    /// Returns a paginated list of published books in the genre identified by <paramref name="slug"/>,
    /// ordered by total views descending.
    /// Returns <c>null</c> when no genre matches the slug.
    /// </summary>
    public async Task<PagedResponse<BookSummaryResponse>?> GetBooksByGenreAsync(
        string slug, int page = 1, int limit = 20)
    {
        // Resolve slug → genre (all genres fit in memory; avoids SQL REPLACE translation issues)
        var allGenres = await db.Genres.ToListAsync();
        var genre = allGenres.FirstOrDefault(g =>
            string.Equals(ToSlug(g.Name), slug, StringComparison.OrdinalIgnoreCase));

        if (genre is null) return null;

        var offset = (page - 1) * limit;

        var total = await db.BookGenres
            .CountAsync(bg => bg.GenreId == genre.Id && bg.Book.Status == "published");

        var items = await db.BookGenres
            .Where(bg => bg.GenreId == genre.Id && bg.Book.Status == "published")
            .OrderByDescending(bg => bg.Book.TotalViews)
            .Skip(offset)
            .Take(limit)
            .Select(bg => new BookSummaryResponse
            {
                Id = bg.Book.Id,
                Title = bg.Book.Title,
                Slug = bg.Book.Slug,
                Description = bg.Book.Description,
                CoverUrl = bg.Book.CoverUrl,
                Language = bg.Book.Language,
                Status = bg.Book.Status,
                TotalViews = bg.Book.TotalViews,
                CreatedAt = bg.Book.CreatedAt,
                AuthorName = bg.Book.Author != null ? bg.Book.Author.DisplayName : null,
                Authors = bg.Book.BookAuthors.Select(ba => ba.AuthorName).ToList(),
                Genres = bg.Book.BookGenres.Select(g => g.Genre.Name).ToList(),
            })
            .ToListAsync();

        return new PagedResponse<BookSummaryResponse>
        {
            Items = items,
            Page = page,
            Limit = limit,
            Total = total,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Converts a genre name to its URL slug (e.g. "Science Fiction" → "science-fiction").</summary>
    public static string ToSlug(string name) =>
        name.ToLowerInvariant()
            .Replace("'", "")
            .Replace(" ", "-");
}
