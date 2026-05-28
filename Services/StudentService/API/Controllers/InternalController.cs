// ══════════════════════════════════════════════════════════════
// INTERNAL CONTROLLER — Service-to-service only
// ══════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Wrappers;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/internal/students")]
[Produces("application/json")]
public class InternalController : ControllerBase
{
    private readonly IStudentProfileService _profileService;
    private readonly IConfiguration _config;

    public InternalController(IStudentProfileService profileService, IConfiguration config)
    {
        _profileService = profileService;
        _config = config;
    }

    /// <summary>
    /// Returns CGPA, branch, college for eligibility check by Application Service.
    /// Protected by internal service secret header — not JWT.
    /// </summary>
    [HttpGet("{userId:guid}/eligibility")]
    [ProducesResponseType(typeof(ApiResponse<StudentEligibilityResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetEligibility(Guid userId, CancellationToken ct)
    {
        if (!IsInternalRequest())
            return StatusCode(403, ApiResponse.Fail("Internal endpoint — access denied."));

        var result = await _profileService.GetEligibilityAsync(userId, ct);
        return Ok(ApiResponse<StudentEligibilityResponse>.Ok(result));
    }

    /// <summary>
    /// Returns lightweight profile summary for Recruiter applicant list.
    /// Protected by internal service secret header — not JWT.
    /// </summary>
    [HttpGet("{userId:guid}/summary")]
    [ProducesResponseType(typeof(ApiResponse<StudentSummaryResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetSummary(Guid userId, CancellationToken ct)
    {
        if (!IsInternalRequest())
            return StatusCode(403, ApiResponse.Fail("Internal endpoint — access denied."));

        var result = await _profileService.GetSummaryAsync(userId, ct);
        return Ok(ApiResponse<StudentSummaryResponse>.Ok(result));
    }

    private bool IsInternalRequest()
    {
        var expectedSecret = _config["InternalServiceSecret"];
        var providedSecret = Request.Headers["X-Internal-Secret"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(expectedSecret)
               && string.Equals(expectedSecret, providedSecret, StringComparison.Ordinal);
    }
}