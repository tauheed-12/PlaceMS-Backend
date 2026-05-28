// ══════════════════════════════════════════════════════════════
// SKILLS CONTROLLER
// ══════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Wrappers;
using StudentService.Application.DTOs.Requests;
using StudentService.Application.Interfaces;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/students/me/skills")]
[Authorize(Roles = Roles.Student)]
[Produces("application/json")]
public class SkillsController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public SkillsController(IStudentProfileService profileService)
        => _profileService = profileService;

    /// <summary>Replace the entire skill list atomically.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
    public async Task<IActionResult> Replace([FromBody] ReplaceSkillsRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.ReplaceSkillsAsync(userId, request, ct);
        return Ok(ApiResponse<List<string>>.Ok(result, "Skills updated."));
    }
}