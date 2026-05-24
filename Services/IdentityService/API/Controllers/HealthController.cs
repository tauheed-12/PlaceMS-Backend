using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/v1/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly IdentityDbContext _context;

    public HealthController(IdentityDbContext context)
        => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try
        {
            await _context.Database.CanConnectAsync(ct);
            return Ok(new
            {
                status = "healthy",
                service = "IdentityService",
                timestamp = DateTime.UtcNow,
                database = "connected"
            });
        }
        catch
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                service = "IdentityService",
                timestamp = DateTime.UtcNow,
                database = "disconnected"
            });
        }
    }
}