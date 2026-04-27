namespace Caesura.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>Genre listing and genre-filtered book browsing.</summary>
[ApiController]
[Route("api/v1/genres")]
[Produces("application/json")]
[Tags("Genres")]
public class GenresController(GenresService genres) : ControllerBase
{
    /// <summary>List all genres with their published-book counts.</summary>
    /// <response code="200">Array of genres ordered by name.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<GenreResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListGenres()
        => Ok(await genres.GetAllGenresAsync());

    /// <summary>Paginated list of published books in a genre.</summary>
    /// <param name="slug">Genre URL slug (e.g. "science-fiction").</param>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <response code="200">Paginated book list ordered by total views.</response>
    /// <response code="404">Genre slug not recognised.</response>
    [HttpGet("{slug}/books")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGenreBooks(string slug, [FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        var result = await genres.GetBooksByGenreAsync(slug, page);
        return result is null
            ? NotFound(new { error = $"Genre '{slug}' not found." })
            : Ok(result);
    }
}
