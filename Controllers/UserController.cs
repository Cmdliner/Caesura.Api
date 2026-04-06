using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController (
    IUserService userService
    ): ControllerBase
{
    
    private readonly IUserService _userService = userService;
    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);


    [HttpGet("books/authored"), Authorize]
    public async Task<IActionResult> GetAuthoredBooks()
    {
        var authoredBooks = await _userService.GetAuthoredBooks(UserId);
        return Ok(authoredBooks);
    }

    [HttpGet("books/authored/{slug}"), Authorize]
    public async Task<IActionResult> GetBook(string slug)
    {
        var result = await _userService.GetBookBySlugAsync(slug, UserId);
        return  result is null ? NotFound(new { error = "Book not found." }) : Ok(result);
    }
}