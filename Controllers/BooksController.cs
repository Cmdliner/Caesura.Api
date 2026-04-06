namespace Caesura.Api.Controllers;

using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Caesura.Api.DTOs.Books;
using Caesura.Api.Infrastructure;
using Caesura.Api.Services;

[ApiController]
[Route("api/v1/books")]
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

    // books

    [HttpGet]
    public async Task<IActionResult> ListBooks([FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        return Ok(await books.ListBooksAsync(page));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBook(string slug)
    {
        var result = await books.GetBookBySlugAsync(slug);
        return result is null ? NotFound(new { error = "Book not found." }) : Ok(result);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        var check = await this.ValidateAsync(createBookValidator, request);
        if (check is not null) return check;

        var book = await books.CreateBookAsync(UserId, request);
        var response = new CreateBookResponse
        {
            BookId = book.Id,
            Title = book.Title
        };
    
        return CreatedAtAction(nameof(GetBook), new { slug = book.Slug }, response);
    }

    [HttpPatch("{id:guid}"), Authorize]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookRequest request)
    {
        var check = await this.ValidateAsync(updateBookValidator, request);
        if (check is not null) return check;

        var book = await books.UpdateBookAsync(id, UserId, request);
        return book is null
            ? NotFound(new { error = "Book not found or you are not the author." })
            : Ok(new { book });
    }

    // chapters
    [HttpGet("{bookId:guid}/chapters/{chapterNumber:int}")]
    public async Task<IActionResult> GetChapter(Guid bookId, int chapterNumber)
    {
        if (chapterNumber < 1)
            return BadRequest(new { error = "Chapter number must be greater than 0." });

        var chapter = await books.GetChapterAsync(bookId, chapterNumber);
        return chapter is null ? NotFound(new { error = "Chapter not found." }) : Ok(new { chapter });
    }

    [HttpPost("{bookId:guid}/chapters"), Authorize]
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

    [HttpPatch("{bookId:guid}/chapters/{chapterId:guid}"), Authorize]
    public async Task<IActionResult> UpdateChapter(
        Guid bookId, Guid chapterId, [FromBody] UpdateChapterRequest request)
    {
        var check = await this.ValidateAsync(updateChapterValidator, request);
        if (check is not null) return check;

        var chapter = await books.UpdateChapterAsync(bookId, chapterId, UserId, request);
        return chapter is null ? NotFound(new { error = "Chapter not found." }) : Ok(new { chapter });
    }

    // library 
    [HttpGet("library/me"), Authorize]
    public async Task<IActionResult> GetLibrary()
        => Ok(await books.GetUserLibraryAsync(UserId));

    [HttpPost("{bookId:guid}/library"), Authorize]
    public async Task<IActionResult> AddToLibrary(Guid bookId)
    {
        await books.AddToLibraryAsync(UserId, bookId);
        return Ok(new { success = true });
    }

    [HttpDelete("{bookId:guid}/library"), Authorize]
    public async Task<IActionResult> RemoveFromLibrary(Guid bookId)
    {
        await books.RemoveFromLibraryAsync(UserId, bookId);
        return Ok(new { success = true });
    }

    // reading progress
    [HttpGet("{bookId:guid}/progress"), Authorize]
    public async Task<IActionResult> GetProgress(Guid bookId)
        => Ok(new { progress = await books.GetProgressAsync(UserId, bookId) });

    [HttpPost("{bookId:guid}/progress"), Authorize]
    public async Task<IActionResult> UpdateProgress(
        Guid bookId, [FromBody] UpdateProgressRequest request)
    {
        var check = await this.ValidateAsync(progressValidator, request);
        if (check is not null) return check;

        await books.UpsertProgressAsync(UserId, bookId, request);
        return Ok(new { success = true });
    }

    // Inline comments 
    [HttpGet("chapters/{chapterId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid chapterId)
        => Ok(await books.GetInlineCommentsAsync(chapterId));

    [HttpPost("chapters/{chapterId:guid}/comments"), Authorize]
    public async Task<IActionResult> PostComment(
        Guid chapterId, [FromBody] InlineCommentRequest request)
    {
        var check = await this.ValidateAsync(commentValidator, request);
        if (check is not null) return check;

        var comment = await books.CreateInlineCommentAsync(chapterId, UserId, request);
        return CreatedAtAction(nameof(GetComments), new { chapterId }, new { comment });
    }

    // ratings

    [HttpPost("{bookId:guid}/rate"), Authorize]
    public async Task<IActionResult> RateBook(Guid bookId, [FromBody] RateBookRequest request)
    {
        var check = await this.ValidateAsync(ratingValidator, request);
        if (check is not null) return check;

        return Ok(await books.RateBookAsync(UserId, bookId, request));
    }
}