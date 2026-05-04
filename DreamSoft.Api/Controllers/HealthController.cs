using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

/// <summary>
/// Lightweight health check endpoint used by Railway to verify the container
/// is up and ready to serve traffic before marking a deployment as healthy.
/// Intentionally unversioned and requires no authentication.
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}
