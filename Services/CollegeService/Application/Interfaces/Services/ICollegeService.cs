using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;

namespace CollegeService.Application.Interfaces.Services;

public interface ICollegeService
{
    Task<CreateCollegeResponseDto> RegisterAsync(CreateCollegeRequestDto request, string registeredBy, CancellationToken ct);
    // Task VerifyCollegeEmailAsync(string token, CancellationToken ct);
    Task<UpdateCollegeResponseDto> UpdateAsync(UpdateCollegeRequestDto request, string updatedBy, CancellationToken ct);
    Task DeactivateCollegeAsync(Guid collegeId, CancellationToken ct);
    Task ReactivateCollegeAsync(Guid collegeId, CancellationToken ct);
}