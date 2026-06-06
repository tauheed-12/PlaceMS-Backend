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
[Route("api/v1/tpo/drives")]
[Authorize(Roles = Roles.TPOOrCoordinator)]
[Produces("application/json")]
public class TpoDriveController : ControllerBase
{
    private readonly IDriveService _driveService;

    public TpoDriveController(IDriveService driveService) => _driveService = driveService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TpoDriveResponse>>), 200)]
    public async Task<IActionResult> GetCollegeDrives([FromQuery] DriveListQuery query, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("TPO account is not associated with a college.");
        var result = await _driveService.GetCollegeDrivesAsync(collegeId, query, ct);
        return Ok(ApiResponse<PagedResult<TpoDriveResponse>>.Ok(result));
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TpoDriveResponse>>), 200)]
    public async Task<IActionResult> GetPendingCollegeDrives([FromQuery] DriveListQuery query, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("TPO account is not associated with a college.");
        var result = await _driveService.GetPendingCollegeDrivesAsync(collegeId, query, ct);
        return Ok(ApiResponse<PagedResult<TpoDriveResponse>>.Ok(result));
    }

    [HttpPost("{driveId:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<DriveApprovalActionResponse>), 200)]
    public async Task<IActionResult> ApproveDrive(Guid driveId, [FromBody] ApproveDriveRequest request, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("TPO account is not associated with a college.");
        var tpoUserId = User.GetUserId();
        var result = await _driveService.ApproveDriveAsync(driveId, collegeId, tpoUserId, request, ct);
        return Ok(ApiResponse<DriveApprovalActionResponse>.Ok(result, "Drive approved successfully."));
    }

    [HttpPost("{driveId:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<DriveApprovalActionResponse>), 200)]
    public async Task<IActionResult> RejectDrive(Guid driveId, [FromBody] RejectDriveRequest request, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("TPO account is not associated with a college.");
        var tpoUserId = User.GetUserId();
        var result = await _driveService.RejectDriveAsync(driveId, collegeId, tpoUserId, request, ct);
        return Ok(ApiResponse<DriveApprovalActionResponse>.Ok(result, "Drive rejected successfully."));
    }

    [HttpPost("{driveId:guid}/request-changes")]
    [ProducesResponseType(typeof(ApiResponse<DriveApprovalActionResponse>), 200)]
    public async Task<IActionResult> RequestChanges(Guid driveId, [FromBody] RequestChangesRequest request, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("TPO account is not associated with a college.");
        var tpoUserId = User.GetUserId();
        var result = await _driveService.RequestChangesAsync(driveId, collegeId, tpoUserId, request, ct);
        return Ok(ApiResponse<DriveApprovalActionResponse>.Ok(result, "Drive changes requested successfully."));
    }
}
