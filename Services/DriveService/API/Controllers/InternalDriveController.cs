using DriveService.Application.DTOs.Responses;
using DriveService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Wrappers;

namespace DriveService.API.Controllers;

[ApiController]
[Route("api/v1/internal/drives")]
[Produces("application/json")]
public class InternalDriveController : ControllerBase
{
    private readonly IDriveService _driveService;
    private readonly IConfiguration _config;

    public InternalDriveController(IDriveService driveService, IConfiguration config)
    {
        _driveService = driveService;
        _config = config;
    }

    [HttpGet("{driveId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InternalDriveDetailResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetInternalDriveDetail(Guid driveId, CancellationToken ct)
    {
        if (!IsInternalRequest())
            return StatusCode(403, ApiResponse.Fail("Internal endpoint — access denied."));

        var result = await _driveService.GetInternalDriveDetailAsync(driveId, ct);
        return Ok(ApiResponse<InternalDriveDetailResponse>.Ok(result));
    }

    [HttpGet("{driveId:guid}/college-status/{collegeId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DriveCollegeStatusResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetDriveCollegeStatus(Guid driveId, Guid collegeId, CancellationToken ct)
    {
        if (!IsInternalRequest())
            return StatusCode(403, ApiResponse.Fail("Internal endpoint — access denied."));

        var result = await _driveService.GetDriveCollegeStatusAsync(driveId, collegeId, ct);
        return Ok(ApiResponse<DriveCollegeStatusResponse>.Ok(result));
    }

    private bool IsInternalRequest()
    {
        var expectedSecret = _config["InternalServiceSecret"];
        var providedSecret = Request.Headers["X-Internal-Secret"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(expectedSecret)
               && string.Equals(expectedSecret, providedSecret, StringComparison.Ordinal);
    }
}
