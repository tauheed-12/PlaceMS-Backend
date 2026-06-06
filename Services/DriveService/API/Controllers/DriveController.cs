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
[Route("api/v1/drives")]
[Authorize]
[Produces("application/json")]
public class DriveController : ControllerBase
{
    private readonly IDriveService _driveService;

    public DriveController(IDriveService driveService) => _driveService = driveService;

    [HttpPost]
    [Authorize(Roles = Roles.Recruiter)]
    [ProducesResponseType(typeof(ApiResponse<CreateDriveResponse>), 201)]
    public async Task<IActionResult> CreateDrive([FromBody] CreateDriveRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _driveService.CreateDriveAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetDriveById), new { driveId = result.DriveId }, ApiResponse<CreateDriveResponse>.Created(result, "Drive created successfully."));
    }

    [HttpGet]
    [Authorize(Roles = Roles.Recruiter)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DriveListItemResponse>>), 200)]
    public async Task<IActionResult> GetMyDrives([FromQuery] DriveListQuery query, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _driveService.GetRecruiterDrivesAsync(userId, query, ct);
        return Ok(ApiResponse<PagedResult<DriveListItemResponse>>.Ok(result));
    }

    [HttpGet("{driveId:guid}")]
    [Authorize(Roles = $"{Roles.Recruiter},{Roles.Admin},{Roles.SuperAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<DriveDetailResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetDriveById(Guid driveId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _driveService.GetDriveDetailAsync(driveId, userId, ct);
        return Ok(ApiResponse<DriveDetailResponse>.Ok(result));
    }

    [HttpPut("{driveId:guid}")]
    [Authorize(Roles = Roles.Recruiter)]
    [ProducesResponseType(typeof(ApiResponse<DriveDetailResponse>), 200)]
    public async Task<IActionResult> UpdateDrive(Guid driveId, [FromBody] UpdateDriveRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _driveService.UpdateDriveAsync(driveId, userId, request, ct);
        return Ok(ApiResponse<DriveDetailResponse>.Ok(result, "Drive updated successfully."));
    }

    [HttpDelete("{driveId:guid}")]
    [Authorize(Roles = $"{Roles.Recruiter},{Roles.Admin},{Roles.SuperAdmin}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> DeactivateDrive(Guid driveId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        await _driveService.DeactivateDriveAsync(driveId, userId, role, ct);
        return Ok(ApiResponse.Ok("Drive deactivated successfully."));
    }

    [HttpPost("{driveId:guid}/resubmit/{collegeId:guid}")]
    [Authorize(Roles = Roles.Recruiter)]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> ResubmitDrive(Guid driveId, Guid collegeId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _driveService.ResubmitToCollegeAsync(driveId, collegeId, userId, ct);
        return Ok(ApiResponse.Ok("Drive resubmitted to college successfully."));
    }
}
