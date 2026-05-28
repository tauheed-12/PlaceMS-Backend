// ══════════════════════════════════════════════════════════════
// CERTIFICATIONS CONTROLLER
// ══════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Wrappers;
using StudentService.Application.DTOs.Requests;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/students/me/certifications")]
[Authorize(Roles = Roles.Student)]
[Produces("application/json")]
public class CertificationsController : ControllerBase
{
    private readonly IStudentProfileService _profileService;

    public CertificationsController(IStudentProfileService profileService)
        => _profileService = profileService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CertificationResponse>), 201)]
    public async Task<IActionResult> Add([FromBody] AddCertificationRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.AddCertificationAsync(userId, request, ct);
        return CreatedAtAction(nameof(Add), ApiResponse<CertificationResponse>.Created(result));
    }

    [HttpPut("{certId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CertificationResponse>), 200)]
    public async Task<IActionResult> Update(
        Guid certId, [FromBody] UpdateCertificationRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _profileService.UpdateCertificationAsync(userId, certId, request, ct);
        return Ok(ApiResponse<CertificationResponse>.Ok(result));
    }

    [HttpDelete("{certId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> Delete(Guid certId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _profileService.DeleteCertificationAsync(userId, certId, ct);
        return Ok(ApiResponse.Ok("Certification removed."));
    }
}