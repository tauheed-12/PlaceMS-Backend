// ══════════════════════════════════════════════════════════════
// PROJECTS CONTROLLER
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
[Route("api/v1/students/me/projects")]
[Authorize(Roles = Roles.Student)]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public ProjectsController(IStudentProfileService profileService)
        => _profileService = profileService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 201)]
    public async Task<IActionResult> Add([FromBody] AddProjectRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.AddProjectAsync(userId, request, ct);
        return CreatedAtAction(nameof(Add), ApiResponse<ProjectResponse>.Created(result));
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 200)]
    public async Task<IActionResult> Update(
        Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.UpdateProjectAsync(userId, projectId, request, ct);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _profileService.DeleteProjectAsync(userId, projectId, ct);
        return Ok(ApiResponse.Ok("Project removed."));
    }
}