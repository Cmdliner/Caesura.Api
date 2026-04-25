using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Controllers;

/// <summary>Endpoints scoped to the currently authenticated user.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("User")]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>List all books authored by the current user.</summary>
    /// <response code="200">Array of authored books.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("books/authored")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuthoredBooks()
    {
        var authoredBooks = await _userService.GetAuthoredBooks(UserId);
        return Ok(authoredBooks);
    }

    /// <summary>Get a single authored book by its slug.</summary>
    /// <param name="slug">URL-safe book identifier.</param>
    /// <response code="200">Book details.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="404">Book not found or not owned by the current user.</response>
    [HttpGet("books/authored/{slug}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBook(string slug)
    {
        var result = await _userService.GetBookBySlugAsync(slug, UserId);
        return result is null
            ? NotFound(new { error = "Book not found." })
            : Ok(result);
    }

    /// <summary>Get a chapter's full content for editing. Only the book author may call this.</summary>
    /// <param name="bookId">Book GUID.</param>
    /// <param name="chapterNumber">1-based chapter index.</param>
    /// <response code="200">Chapter detail including draft content.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="404">Chapter not found or not owned by the current user.</response>
    [HttpGet("books/{bookId:guid}/chapters/{chapterNumber:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterForEdit(Guid bookId, int chapterNumber)
    {
        var chapter = await _userService.GetChapterForEditAsync(bookId, UserId, chapterNumber);
        return chapter is null
            ? NotFound(new { error = "Chapter not found." })
            : Ok(new { chapter });
    }
}
