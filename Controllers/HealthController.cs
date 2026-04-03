using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Controllers;

[ApiController()]
[Route("api/v1/health")]
public class HealthController: ControllerBase
{
    [HttpGet()]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy" });
    }
}