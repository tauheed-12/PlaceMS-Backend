using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Models;
using SharedKernel.Wrappers;

namespace CollegeService.API.Controllers;

[Route("api/v1/admin/collegescope")]
[ApiController]
[Authorize(Roles = Roles.SuperAdminOrAdmin)]
public class AdminCollegeScopeController : ControllerBase
{
    private readonly IAdminCollegeScopeService _adminCollegeScopeService;
    private readonly ICollegeTpoService _collegeTpoService;
    private readonly ICollegeQueryService _collegeQueryService;

    public AdminCollegeScopeController(
        IAdminCollegeScopeService adminCollegeScopeService,
        ICollegeTpoService collegeTpoService,
        ICollegeQueryService collegeQueryService)
    {
        _adminCollegeScopeService = adminCollegeScopeService;
        _collegeTpoService = collegeTpoService;
        _collegeQueryService = collegeQueryService;
    }


    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminCollegeScopeResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> CreateCollegeScope([FromBody] AdminCollegeScopeRequestDto collegeScope, CancellationToken ct)
    {
        var response = await _adminCollegeScopeService.AssignCollegeToAdminAsync(collegeScope.AdminId, collegeScope.CollegeId, ct);
        return CreatedAtAction(nameof(CreateCollegeScope), null, ApiResponse<AdminCollegeScopeResponseDto>.Created(response));
    }


    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse), 204)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> RemoveCollegeScope([FromBody] AdminCollegeScopeRequestDto collegeScope, CancellationToken ct)
    {
        await _adminCollegeScopeService.RemoveCollegeFromAdminAsync(collegeScope.AdminId, collegeScope.CollegeId, ct);
        return NoContent();
    }


    // Super admin can view all TPOs and colleges under an admin's scope
    [HttpGet("{adminId}/tpos")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetTposByAdminId(Guid adminId, TpoFilterRequestDto filter, CancellationToken ct)
    {
        var tpoDetails = await _adminCollegeScopeService.GetTposByAdminIdAsync(adminId, filter, ct);
        return Ok(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>.Ok(tpoDetails));
    }


    [HttpGet("{adminId}/colleges")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<CollegeShortDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetCollegesByAdminId(Guid adminId, CollegeFilterRequestDto filter, CancellationToken ct)
    {
        var collegeDetails = await _adminCollegeScopeService.GetCollegesByAdminIdAsync(adminId, filter, ct);
        return Ok(ApiResponse<PaginatedResponseDto<CollegeShortDto>>.Ok(collegeDetails));
    }

    [HttpGet("superadmin/admins")]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<AdminDetailsDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetAdmins(AdminFilterRequestDto filter, CancellationToken ct)
    {
        var admins = await _adminCollegeScopeService.GetAdminsAsync(filter, ct);
        return Ok(ApiResponse<PaginatedResponseDto<AdminDetailsDto>>.Ok(admins));
    }

    [HttpGet("superadmin/tpos")]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetAllTpos([FromQuery] TpoFilterRequestDto filter, CancellationToken ct)
    {
        var tpoDetails = await _collegeTpoService.GetTposAsync(filter, ct);
        return Ok(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>.Ok(tpoDetails));
    }

    [HttpGet("superadmin/colleges")]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ProducesResponseType(typeof(PagedResult<CollegeShortDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetAllColleges([FromQuery] CollegeFilterRequestDto filter, CancellationToken ct)
    {
        var result = await _collegeQueryService.GetFilteredCollegesAsync(filter, ct);
        return Ok(result);
    }


    [HttpGet("me/tpos")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetTposByAdminId(TpoFilterRequestDto filter, CancellationToken ct)
    {
        var adminId = Guid.NewGuid(); // Replace with actual admin ID retrieval logic
        var tpoDetails = await _adminCollegeScopeService.GetTposByAdminIdAsync(adminId, filter, ct);
        return Ok(ApiResponse<PaginatedResponseDto<TpoDetailsDto>>.Ok(tpoDetails));
    }


    [HttpGet("me/colleges")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<CollegeShortDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetCollegesByAdminId(CollegeFilterRequestDto filter, CancellationToken ct)
    {
        var adminId = Guid.NewGuid(); // Replace with actual admin ID retrieval logic
        var collegeDetails = await _adminCollegeScopeService.GetCollegesByAdminIdAsync(adminId, filter, ct);
        return Ok(ApiResponse<PaginatedResponseDto<CollegeShortDto>>.Ok(collegeDetails));
    }
}