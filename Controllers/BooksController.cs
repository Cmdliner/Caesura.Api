namespace Caesura.Api.Controllers;

using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Caesura.Api.DTOs.Books;
using Caesura.Api.Infrastructure;
using Caesura.Api.Services;

/// <summary>Books, chapters, library, reading progress, comments and ratings.</summary>
[ApiController]
[Route("api/v1/books")]
[Produces("application/json")]
[Tags("Books")]
public class BooksController(
    BooksService books,
    IValidator<CreateBookRequest> createBookValidator,
    IValidator<UpdateBookRequest> updateBookValidator,
    IValidator<CreateChapterRequest> createChapterValidator,
    IValidator<UpdateChapterRequest> updateChapterValidator,
    IValidator<UpdateProgressRequest> progressValidator,
    IValidator<InlineCommentRequest> commentValidator,
    IValidator<RateBookRequest> ratingValidator) : ControllerBase
{
    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    // ── Books ─────────────────────────────────────────────────────────────────

    /// <summary>Paginated list of all published books.</summary>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <response code="200">Paginated book list.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBooks([FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        return Ok(await books.ListBooksAsync(page));
    }

    /// <summary>Get a single book by its URL slug.</summary>
    /// <param name="slug">URL-safe book identifier.</param>
    /// <response code="200">Book detail with chapters list.</response>
    /// <response code="404">Book not found.</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBook(string slug)
    {
        var result = await books.GetBookBySlugAsync(slug);
        return result is null ? NotFound(new { error = "Book not found." }) : Ok(result);
    }

    /// <summary>Create a new book. The authenticated user becomes the author.</summary>
    /// <param name="request">Book metadata (title, description, genre, cover URL).</param>
    /// <response code="201">Book created. Location header points to the new resource.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateBookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        var check = await this.ValidateAsync(createBookValidator, request);
        if (check is not null) return check;

        var book = await books.CreateBookAsync(UserId, request);
        var response = new CreateBookResponse { BookId = book.Id, Title = book.Title, Slug = book.Slug };
        return CreatedAtAction(nameof(GetBook), new { slug = book.Slug }, response);
    }

    /// <summary>Update book metadata. Only the author may call this.</summary>
    /// <param name="id">Book GUID.</param>
    /// <param name="request">Fields to update (all optional).</param>
    /// <response code="200">Updated book object.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="404">Book not found or caller is not the author.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPatch("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookRequest request)
    {
        var check = await this.ValidateAsync(updateBookValidator, request);
        if (check is not null) return check;

        var book = await books.UpdateBookAsync(id, UserId, request);
        return book is null
            ? NotFound(new { error = "Book not found or you are not the author." })
            : Ok(new { book });
    }

    // ── Chapters ──────────────────────────────────────────────────────────────

    /// <summary>Get a single chapter by book and chapter number.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <param name="chapterNumber">1-based chapter index.</param>
    /// <response code="200">Chapter content.</response>
    /// <response code="400">Chapter number must be ≥ 1.</response>
    /// <response code="404">Chapter not found.</response>
    [HttpGet("{bookId:guid}/chapters/{chapterNumber:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapter(Guid bookId, int chapterNumber)
    {
        if (chapterNumber < 1)
            return BadRequest(new { error = "Chapter number must be greater than 0." });

        var chapter = await books.GetChapterAsync(bookId, chapterNumber);
        return chapter is null ? NotFound(new { error = "Chapter not found." }) : Ok(new { chapter });
    }

    /// <summary>Add a new chapter to a book. Only the book author may call this.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <param name="request">Chapter title and content.</param>
    /// <response code="201">Chapter created.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Caller is not the book author.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost("{bookId:guid}/chapters")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateChapter(Guid bookId, [FromBody] CreateChapterRequest request)
    {
        var check = await this.ValidateAsync(createChapterValidator, request);
        if (check is not null) return check;

        var chapter = await books.CreateChapterAsync(bookId, UserId, request);
        return chapter is null
            ? Forbid()
            : CreatedAtAction(
                nameof(GetChapter),
                new { bookId, chapterNumber = chapter.ChapterNumber },
                new { chapter });
    }

    /// <summary>Update chapter content. Only the book author may call this.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <param name="chapterId">Chapter GUID.</param>
    /// <param name="request">Fields to update.</param>
    /// <response code="200">Updated chapter.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="404">Chapter not found.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPatch("{bookId:guid}/chapters/{chapterId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateChapter(
        Guid bookId, Guid chapterId, [FromBody] UpdateChapterRequest request)
    {
        var check = await this.ValidateAsync(updateChapterValidator, request);
        if (check is not null) return check;

        var chapter = await books.UpdateChapterAsync(bookId, chapterId, UserId, request);
        return chapter is null
            ? NotFound(new { error = "Chapter not found." })
            : Ok(new { chapter });
    }

    /// <summary>Delete a chapter. Only the book author may call this.</summary>
    [HttpDelete("{bookId:guid}/chapters/{chapterId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChapter(Guid bookId, Guid chapterId)
    {
        var deleted = await books.DeleteChapterAsync(bookId, chapterId, UserId);
        return deleted ? NoContent() : NotFound(new { error = "Chapter not found." });
    }

    // ── Library ───────────────────────────────────────────────────────────────

    /// <summary>Get the authenticated user's saved library.</summary>
    /// <response code="200">Array of library items with reading progress.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("library/me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLibrary()
        => Ok(await books.GetUserLibraryAsync(UserId));

    /// <summary>Add a book to the authenticated user's library.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <response code="200">Book added to library.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpPost("{bookId:guid}/library")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToLibrary(Guid bookId)
    {
        await books.AddToLibraryAsync(UserId, bookId);
        return Ok(new { success = true });
    }

    /// <summary>Remove a book from the authenticated user's library.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <response code="200">Book removed from library.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpDelete("{bookId:guid}/library")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFromLibrary(Guid bookId)
    {
        await books.RemoveFromLibraryAsync(UserId, bookId);
        return Ok(new { success = true });
    }

    // ── Reading progress ──────────────────────────────────────────────────────

    /// <summary>Get the current user's reading progress for a book.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <response code="200">Progress object (scroll position, last read timestamp).</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("{bookId:guid}/progress")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProgress(Guid bookId)
        => Ok(new { progress = await books.GetProgressAsync(UserId, bookId) });

    /// <summary>Upsert reading progress for a book.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <param name="request">Scroll position (0–100) and chapter reference.</param>
    /// <response code="200">Progress saved.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost("{bookId:guid}/progress")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateProgress(
        Guid bookId, [FromBody] UpdateProgressRequest request)
    {
        var check = await this.ValidateAsync(progressValidator, request);
        if (check is not null) return check;

        await books.UpsertProgressAsync(UserId, bookId, request);
        return Ok(new { success = true });
    }

    // ── Inline comments ───────────────────────────────────────────────────────

    /// <summary>List all inline comments on a chapter.</summary>
    /// <param name="chapterId">Chapter GUID.</param>
    /// <response code="200">Array of inline comments.</response>
    [HttpGet("chapters/{chapterId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(Guid chapterId)
        => Ok(await books.GetInlineCommentsAsync(chapterId));

    /// <summary>Post an inline comment on a chapter.</summary>
    /// <param name="chapterId">Chapter GUID.</param>
    /// <param name="request">Comment text and optional anchor position.</param>
    /// <response code="201">Comment created.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost("chapters/{chapterId:guid}/comments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PostComment(
        Guid chapterId, [FromBody] InlineCommentRequest request)
    {
        var check = await this.ValidateAsync(commentValidator, request);
        if (check is not null) return check;

        var comment = await books.CreateInlineCommentAsync(chapterId, UserId, request);
        return CreatedAtAction(nameof(GetComments), new { chapterId }, new { comment });
    }

    // ── Ratings ───────────────────────────────────────────────────────────────

    /// <summary>Rate a book (1–5 stars). Upserts an existing rating.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <param name="request">Star rating value.</param>
    /// <response code="200">Updated aggregate rating.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost("{bookId:guid}/rate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RateBook(Guid bookId, [FromBody] RateBookRequest request)
    {
        var check = await this.ValidateAsync(ratingValidator, request);
        if (check is not null) return check;

        return Ok(await books.RateBookAsync(UserId, bookId, request));
    }
}
