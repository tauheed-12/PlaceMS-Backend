using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Wrappers;
using SharedKernel.Models;

namespace CollegeService.API.Controllers;

[ApiController]
[Route("api/v1/colleges")]
[Produces("application/json")]
public class CollegeController : ControllerBase
{
    private readonly ICollegeService _collegeService;
    private readonly ICollegeQueryService _collegeQueryService;
    private readonly ILogger<CollegeController> _logger;

    public CollegeController(ICollegeService collegeService, ICollegeQueryService collegeQueryService, ILogger<CollegeController> logger)
    {
        _collegeService = collegeService;
        _collegeQueryService = collegeQueryService;
        _logger = logger;
    }

    // Register a new college
    [HttpPost("register")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CreateCollegeResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> RegisterCollege([FromBody] CreateCollegeRequestDto request, CancellationToken ct)
    {
        string registeredBy = "Unknown";
        var result = await _collegeService.RegisterAsync(request, registeredBy, ct);

        return CreatedAtAction(
            nameof(RegisterCollege),
            ApiResponse<CreateCollegeResponseDto>.Created(result)
        );
    }


    // Update college details
    [HttpPut("update")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<UpdateCollegeResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdateCollege([FromBody] UpdateCollegeRequestDto request, CancellationToken ct)
    {
        string updatedBy = "Unknown";
        var result = await _collegeService.UpdateAsync(request, updatedBy, ct);

        return Ok(ApiResponse<UpdateCollegeResponseDto>.Ok(result));
    }


    // Deativate a college
    [HttpDelete("deactivate")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse), 204)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeactivateCollege([FromQuery] Guid collegeId, CancellationToken ct)
    {
        await _collegeService.DeactivateCollegeAsync(collegeId, ct);
        return NoContent();
    }


    // Activate a college
    [HttpPut("activate")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> ActivateCollege([FromQuery] Guid collegeId, CancellationToken ct)
    {
        await _collegeService.ReactivateCollegeAsync(collegeId, ct);
        return Ok(ApiResponse.Ok("College reactivated successfully"));
    }


    // Get college details under a admin
    // [HttpGet("admin/{id}")]
    // [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    // [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<CollegeShortDto>>), 200)]
    // [ProducesResponseType(typeof(ApiResponse), 404)]
    // public async Task<IActionResult> GetCollegesByAdminId(CancellationToken ct, [FromRoute] Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    // {
    //     var result = await _collegeQueryService.GetCollegesByAdminIdAsync(id, pageNumber, pageSize, ct);
    //     return Ok(ApiResponse<PaginatedResponseDto<CollegeShortDto>>.Ok(result));
    // }


    // Get all colleges  
    [HttpGet]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<CollegeShortDto>>), 200)]
    public async Task<IActionResult> GetAllColleges(CancellationToken ct, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _collegeQueryService.GetAllCollegesAsync(pageNumber, pageSize, ct);
        return Ok(ApiResponse<PaginatedResponseDto<CollegeShortDto>>.Ok(result));
    }


    // Get college details by id
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.SuperAdminOrAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CollegeDetailsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetCollegeById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _collegeQueryService.GetCollegeByIdAsync(id, ct);
        return Ok(ApiResponse<CollegeDetailsDto>.Ok(result));
    }

    // Search colleges with filters
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<CollegeShortDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetColleges([FromQuery] CollegeFilterRequestDto filter, CancellationToken ct)
    {
        var result = await _collegeQueryService.GetFilteredCollegesAsync(filter, ct);
        return Ok(result);
    }

    // Validate college by code
    [HttpGet("validate")]
    [ProducesResponseType(typeof(ApiResponse<ValidateCollegeCodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateCollege([FromQuery] string code, CancellationToken ct)
    {
        var result = await _collegeQueryService.ValidateCollegeAsync(code, ct);
        return Ok(ApiResponse<ValidateCollegeCodeResponseDto>.Ok(result));
    }
}