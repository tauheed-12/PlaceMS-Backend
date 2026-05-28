using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Models;
using SharedKernel.Wrappers;
using StudentService.Application.DTOs.Requests;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;

namespace StudentService.API.Controllers;

// ══════════════════════════════════════════════════════════════
// STUDENT PROFILE CONTROLLER
// ══════════════════════════════════════════════════════════════

[ApiController]
[Route("api/v1/students")]
[Authorize]
[Produces("application/json")]
public class StudentProfileController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public StudentProfileController(IStudentProfileService profileService)
        => _profileService = profileService;

    /// <summary>Get the authenticated student's own full profile.</summary>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Student)]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileResponse>), 200)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.GetMyProfileAsync(userId, ct);
        return Ok(ApiResponse<StudentProfileResponse>.Ok(result));
    }

    /// <summary>Update personal info — triggers completion score recalculation.</summary>
    [HttpPut("me")]
    [Authorize(Roles = Roles.Student)]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileResponse>), 200)]
    public async Task<IActionResult> UpdatePersonalInfo(
        [FromBody] UpdatePersonalInfoRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.UpdatePersonalInfoAsync(userId, request, ct);
        return Ok(ApiResponse<StudentProfileResponse>.Ok(result, "Profile updated successfully."));
    }

    /// <summary>
    /// Get a student's profile by their user ID.
    /// Accessible by TPO (own college only enforced at service level), Recruiter, Admin, SuperAdmin.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [Authorize(Roles = $"{Roles.TPO},{Roles.Recruiter},{Roles.Admin},{Roles.SuperAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<StudentProfileResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetProfileById(Guid userId, CancellationToken ct)
    {
        var result = await _profileService.GetProfileByIdAsync(userId, ct);
        return Ok(ApiResponse<StudentProfileResponse>.Ok(result));
    }

    /// <summary>
    /// Get all students in a college (TPO sees own college, Admin/SuperAdmin specify collegeId).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.TPO},{Roles.Admin},{Roles.SuperAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StudentListItemResponse>>), 200)]
    public async Task<IActionResult> GetStudentsByCollege(
        [FromQuery] Guid? collegeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // TPO can only see their own college
        var callerRole = User.GetRole();
        var resolvedCollegeId = callerRole == Roles.TPO
            ? User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("No college associated with this TPO account.")
            : collegeId ?? throw new SharedKernel.Exceptions.DomainValidationException("collegeId is required for Admin/SuperAdmin.");

        var result = await _profileService.GetStudentsByCollegeAsync(
            resolvedCollegeId, page, pageSize, search, ct);

        return Ok(ApiResponse<PagedResult<StudentListItemResponse>>.Ok(result));
    }
}
