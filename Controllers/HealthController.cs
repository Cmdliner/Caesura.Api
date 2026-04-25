using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Controllers;

/// <summary>Service liveness probe.</summary>
[ApiController]
[Route("api/v1/health")]
[Produces("application/json")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    /// <summary>Returns 200 OK when the service is running.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetHealth() => Ok(new { status = "healthy" });
}
