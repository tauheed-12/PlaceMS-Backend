// ══════════════════════════════════════════════════════════════
// RESUME CONTROLLER
// ══════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Wrappers;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/students/me/resume")]
[Authorize(Roles = Roles.Student)]
[Produces("application/json")]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;

    public ResumeController(IResumeService resumeService)
        => _resumeService = resumeService;

    /// <summary>Upload PDF resume. Replaces any existing active resume.</summary>
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB
    [ProducesResponseType(typeof(ApiResponse<ResumeResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _resumeService.UploadResumeAsync(userId, file, ct);
        return CreatedAtAction(nameof(Get), ApiResponse<ResumeResponse>.Created(result, "Resume uploaded successfully."));
    }

    /// <summary>Get presigned download URL for current active resume.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ResumeResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _resumeService.GetResumeUrlAsync(userId, ct);
        return Ok(ApiResponse<ResumeResponse>.Ok(result));
    }

    /// <summary>Remove current active resume.</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _resumeService.DeleteResumeAsync(userId, ct);
        return Ok(ApiResponse.Ok("Resume removed."));
    }
}