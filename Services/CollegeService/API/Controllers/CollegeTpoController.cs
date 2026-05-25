using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CollegeService.Application.Interfaces.Services;
using SharedKernel.Constants;
using SharedKernel.Wrappers;
using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;

namespace CollegeService.API.Controllers;

[ApiController]
[Route("api/v1/colleges/tpo")]
[Produces("application/json")]
public class CollegeTpoController : ControllerBase
{
    private readonly ICollegeTpoService _collegeTpoService;
    public CollegeTpoController(ICollegeTpoService collegeTpoService)
    {
        _collegeTpoService = collegeTpoService;
    }

    // register a new tpo for a college
    [HttpPost("register")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> RegisterTpo([FromBody] CreateTpoRequestDto request, CancellationToken ct)
    {
        var assignedBy = Guid.NewGuid(); // Replace with actual user ID from auth context
        await _collegeTpoService.AssignPrimaryTpoAsync(request, assignedBy, ct);
        return CreatedAtAction(nameof(RegisterTpo), ApiResponse.Ok("TPO registered successfully."));
    }


    // unregister a tpo from a college
    [HttpDelete("unregister")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UnregisterTpo([FromQuery] Guid collegeId, [FromQuery] Guid userId, CancellationToken ct)
    {
        await _collegeTpoService.RemoveTpoAsync(collegeId, userId, ct);
        return Ok(ApiResponse.Ok("TPO unregistered successfully."));
    }


    // get tpo details for a college
    [HttpGet("details")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetTpoDetails([FromQuery] Guid collegeId, CancellationToken ct)
    {
        var result = await _collegeTpoService.GetPrimaryTpoByCollegeIdAsync(collegeId, ct);
        if (result is null)
            return NotFound(ApiResponse.Fail("TPO not found for the specified college."));

        return Ok(ApiResponse<TpoDetailsDto>.Ok(result));
    }
}