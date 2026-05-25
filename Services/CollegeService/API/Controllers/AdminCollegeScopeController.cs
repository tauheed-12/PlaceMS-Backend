using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Wrappers;

namespace CollegeService.API.Controllers;

[Route("api/admin/collegescope")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminCollegeScopeController : ControllerBase
{
    private readonly IAdminCollegeScopeService _adminCollegeScopeService;

    public AdminCollegeScopeController(IAdminCollegeScopeService adminCollegeScopeService)
    {
        _adminCollegeScopeService = adminCollegeScopeService;
    }
    

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminCollegeScopeResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> CreateCollegeScope([FromBody] AdminCollegeScopeRequestDto collegeScope, CancellationToken ct)
    {
        var response = await _adminCollegeScopeService.AssignCollegeToAdminAsync(collegeScope.AdminId, collegeScope.CollegeId, ct);
        return CreatedAtAction(nameof(CreateCollegeScope), ApiResponse<AdminCollegeScopeResponseDto>.Created(response), null);
    }


    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse), 204)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> RemoveCollegeScope([FromBody] AdminCollegeScopeRequestDto collegeScope, CancellationToken ct)
    {
        await _adminCollegeScopeService.RemoveCollegeFromAdminAsync(collegeScope.AdminId, collegeScope.CollegeId, ct);
        return Ok(ApiResponse.Ok("College scope removed successfully."));
    }


    [HttpGet("{adminId}")]
    [ProducesResponseType(typeof(ApiResponse<List<Guid>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetCollegesByAdminId(Guid adminId, CancellationToken ct)
    {
        var collegeIds = await _adminCollegeScopeService.GetCollegesByAdminIdAsync(adminId, ct);
        return Ok(ApiResponse<List<Guid>>.Ok(collegeIds));
    }
}