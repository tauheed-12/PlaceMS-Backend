using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces.Services;
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


    [HttpGet("{adminId}/tpos")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetTposByAdminId(Guid adminId, CancellationToken ct)
    {
        var tpoDetails = await _adminCollegeScopeService.GetTposByAdminIdAsync(adminId, 1, 10, ct);
        return Ok(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>.Ok(tpoDetails));
    }

    [HttpGet("{adminId}/colleges")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<CollegeShortDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetCollegesByAdminId(Guid adminId, CancellationToken ct)
    {
        var collegeDetails = await _adminCollegeScopeService.GetCollegesByAdminIdAsync(adminId, 1, 10, ct);
        return Ok(ApiResponse<PaginatedResponseDto<CollegeShortDto>>.Ok(collegeDetails));
    }
}