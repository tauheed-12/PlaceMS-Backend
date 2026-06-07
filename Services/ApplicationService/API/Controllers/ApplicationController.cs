using ApplicationService.Application.DTOs.Requests;
using ApplicationService.Application.DTOs.Responses;
using ApplicationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using SharedKernel.Wrappers;

namespace ApplicationService.API.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost("students/{studentId:guid}/applications")]
    public async Task<IActionResult> Apply(Guid studentId, [FromBody] CreateApplicationRequestDto request, CancellationToken ct)
    {
        var result = await _applicationService.ApplyAsync(studentId, request, ct);
        return Ok(ApiResponse<CreateApplicationResponseDto>.Ok(result));
    }

    [HttpPatch("students/{studentId:guid}/applications/{applicationId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid studentId, Guid applicationId, CancellationToken ct)
    {
        var result = await _applicationService.WithdrawAsync(applicationId, studentId, ct);
        return Ok(ApiResponse<WithdrawApplicationResponseDto>.Ok(result));
    }

    [HttpPatch("applications/{applicationId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid applicationId, [FromBody] UpdateApplicationStatusRequestDto request, CancellationToken ct)
    {
        var result = await _applicationService.UpdateStatusAsync(applicationId, request, ct);
        return Ok(ApiResponse<UpdateApplicationStatusResponseDto>.Ok(result));
    }

    [HttpGet("students/{studentId:guid}/applications")]
    public async Task<IActionResult> GetByStudentId(Guid studentId, [FromQuery] ApplicationFilterRequestDto request, CancellationToken ct)
    {
        var result = await _applicationService.GetByStudentIdAsync(studentId, request, ct);
        return Ok(ApiResponse<PagedResult<StudentApplicationDto>>.Ok(result));
    }

    [HttpGet("drives/{driveId:guid}/applications")]
    public async Task<IActionResult> GetByDriveId(Guid driveId, [FromQuery] ApplicationFilterRequestDto request, CancellationToken ct)
    {
        var result = await _applicationService.GetByDriveIdAsync(driveId, request, ct);
        return Ok(ApiResponse<PagedResult<ApplicationDetailsDto>>.Ok(result));
    }

    [HttpGet("colleges/{collegeId:guid}/applications")]
    public async Task<IActionResult> GetByCollegeId(Guid collegeId, [FromQuery] ApplicationFilterRequestDto request, CancellationToken ct)
    {
        var result = await _applicationService.GetByCollegeIdAsync(collegeId, request, ct);
        return Ok(ApiResponse<PagedResult<ApplicationDetailsDto>>.Ok(result));
    }

    [HttpGet("students/{studentId:guid}/applications/{driveId:guid}")]
    public async Task<IActionResult> GetByStudentAndDrive(Guid studentId, Guid driveId, CancellationToken ct)
    {
        var result = await _applicationService.GetByStudentAndDriveAsync(studentId, driveId, ct);
        return Ok(ApiResponse<ApplicationShortDto>.Ok(result!));
    }

    [HttpGet("drives/{driveId:guid}/statistics")]
    public async Task<IActionResult> GetDriveStatistics(Guid driveId, CancellationToken ct)
    {
        var result = await _applicationService.GetDriveStatisticsAsync(driveId, ct);
        return Ok(ApiResponse<ApplicationStatisticsDto>.Ok(result));
    }

    [HttpGet("colleges/{collegeId:guid}/statistics")]
    public async Task<IActionResult> GetCollegeStatistics(Guid collegeId, CancellationToken ct)
    {
        var result = await _applicationService.GetCollegeStatisticsAsync(collegeId, ct);
        return Ok(ApiResponse<ApplicationStatisticsDto>.Ok(result));
    }

    [HttpGet("students/{studentId:guid}/statistics")]
    public async Task<IActionResult> GetStudentStatistics(Guid studentId, CancellationToken ct)
    {
        var result = await _applicationService.GetStudentStatisticsAsync(studentId, ct);
        return Ok(ApiResponse<ApplicationStatisticsDto>.Ok(result));
    }
}
