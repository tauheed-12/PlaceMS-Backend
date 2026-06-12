using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Wrappers;

namespace CollegeService.API.Controllers;

[ApiController]
[Route("api/v1/internal/colleges")]
[Produces("application/json")]
public class InternalCollegeController : ControllerBase
{
    private readonly ICollegeQueryService _collegeQueryService;

    public InternalCollegeController(ICollegeQueryService collegeQueryService)
    {
        _collegeQueryService = collegeQueryService;
    }

    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(ApiResponse<ValidateCollegeCodeResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    public async Task<IActionResult> ValidateCollegeCode(string code, CancellationToken ct)
    {

        var result = await _collegeQueryService.ValidateCollegeAsync(code, ct);
        return Ok(ApiResponse<ValidateCollegeCodeResponseDto>.Ok(result));
    }

    [HttpGet("{code}/status")]
    [ProducesResponseType(typeof(ApiResponse<CollegeStatusResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    public async Task<IActionResult> GetCollegeStatus(string code, CancellationToken ct)
    {
        var result = await _collegeQueryService.ValidateCollegeAsync(code, ct);
        var statusResponse = new CollegeStatusResponse
        {
            CollegeCode = result.CollegeCode,
            IsValid = result.IsValid,
            IsActive = result.IsActive,
            AccountStatus = result.AccountStatus
        };

        return Ok(ApiResponse<CollegeStatusResponse>.Ok(statusResponse));
    }


    private record CollegeStatusResponse
    {
        public string CollegeCode { get; init; } = string.Empty;
        public bool IsValid { get; init; }
        public bool IsActive { get; init; }
        public AccountStatus AccountStatus { get; init; }
    }
}
