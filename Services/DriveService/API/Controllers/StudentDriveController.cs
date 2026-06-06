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
[Route("api/v1/student/drives")]
[Authorize(Roles = Roles.Student)]
[Produces("application/json")]
public class StudentDriveController : ControllerBase
{
    private readonly IDriveService _driveService;

    public StudentDriveController(IDriveService driveService) => _driveService = driveService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StudentDriveResponse>>), 200)]
    public async Task<IActionResult> GetAvailableDrives([FromQuery] AvailableDrivesQuery query, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("Student account is not associated with a college.");
        var result = await _driveService.GetAvailableDrivesAsync(collegeId, query, ct);
        return Ok(ApiResponse<PagedResult<StudentDriveResponse>>.Ok(result));
    }

    [HttpGet("{driveId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDriveResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetDriveById(Guid driveId, CancellationToken ct)
    {
        var collegeId = User.GetCollegeId() ?? throw new SharedKernel.Exceptions.ForbiddenException("Student account is not associated with a college.");
        var result = await _driveService.GetStudentDriveDetailAsync(driveId, collegeId, ct);
        return Ok(ApiResponse<StudentDriveResponse>.Ok(result));
    }
}
