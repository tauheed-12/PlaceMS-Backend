using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;

namespace CollegeService.Application.Interfaces.Services;

public interface ICollegeService
{
    Task<CreateCollegeResponseDto> RegisterAsync(CreateCollegeRequestDto request, Guid registeredBy, CancellationToken ct);
    // Task VerifyCollegeEmailAsync(string token, CancellationToken ct);
    Task<UpdateCollegeResponseDto> UpdateAsync(UpdateCollegeRequestDto request, Guid updatedBy, CancellationToken ct);
    Task DeactivateCollegeAsync(Guid collegeId, Guid deactivatedBy, CancellationToken ct);
    Task ReactivateCollegeAsync(Guid collegeId, Guid activatedBy, CancellationToken ct);
}