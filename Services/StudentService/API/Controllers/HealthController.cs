// ══════════════════════════════════════════════════════════════
// HEALTH CONTROLLER
// ══════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentService.Infrastructure.Persistence;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly StudentDbContext _context;

    public HealthController(StudentDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try
        {
            await _context.Database.CanConnectAsync(ct);
            return Ok(new
            {
                status = "healthy",
                service = "StudentService",
                timestamp = DateTime.UtcNow,
                database = "connected"
            });
        }
        catch
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                service = "StudentService",
                timestamp = DateTime.UtcNow,
                database = "disconnected"
            });
        }
    }
}