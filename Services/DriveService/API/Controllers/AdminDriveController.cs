using DriveService.Application.DTOs.Requests;
using DriveService.Application.DTOs.Responses;
using DriveService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Models;
using SharedKernel.Wrappers;

namespace DriveService.API.Controllers;

[ApiController]
[Route("api/v1/admin/drives")]
[Authorize(Roles = Roles.SuperAdminOrAdmin)]
[Produces("application/json")]
public class AdminDriveController : ControllerBase
{
    private readonly IDriveService _driveService;

    public AdminDriveController(IDriveService driveService) => _driveService = driveService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DriveListItemResponse>>), 200)]
    public async Task<IActionResult> GetAllDrives([FromQuery] AdminDriveListQuery query, CancellationToken ct)
    {
        var result = await _driveService.GetAllDrivesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<DriveListItemResponse>>.Ok(result));
    }

    [HttpGet("{driveId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DriveDetailResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetDriveById(Guid driveId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _driveService.GetDriveDetailAsync(driveId, userId, ct);
        return Ok(ApiResponse<DriveDetailResponse>.Ok(result));
    }

    [HttpGet("{driveId:guid}/college-status/{collegeId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DriveCollegeStatusResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetDriveCollegeStatus(Guid driveId, Guid collegeId, CancellationToken ct)
    {
        var result = await _driveService.GetDriveCollegeStatusAsync(driveId, collegeId, ct);
        return Ok(ApiResponse<DriveCollegeStatusResponse>.Ok(result));
    }
}
