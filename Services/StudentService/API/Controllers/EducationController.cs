// ══════════════════════════════════════════════════════════════
// EDUCATION CONTROLLER
// ══════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Wrappers;
using StudentService.Application.DTOs.Requests;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/students/me/education")]
[Authorize(Roles = Roles.Student)]
[Produces("application/json")]
public class EducationController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public EducationController(IStudentProfileService profileService)
        => _profileService = profileService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EducationResponse>), 201)]
    public async Task<IActionResult> Add([FromBody] AddEducationRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.AddEducationAsync(userId, request, ct);
        return CreatedAtAction(nameof(Add), ApiResponse<EducationResponse>.Created(result));
    }

    [HttpPut("{educationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EducationResponse>), 200)]
    public async Task<IActionResult> Update(
        Guid educationId, [FromBody] UpdateEducationRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.UpdateEducationAsync(userId, educationId, request, ct);
        return Ok(ApiResponse<EducationResponse>.Ok(result));
    }

    [HttpDelete("{educationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> Delete(Guid educationId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _profileService.DeleteEducationAsync(userId, educationId, ct);
        return Ok(ApiResponse.Ok("Education entry removed."));
    }
}