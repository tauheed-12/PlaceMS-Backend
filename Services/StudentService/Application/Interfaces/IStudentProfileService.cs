using StudentService.Application.DTOs.Requests;
using StudentService.Application.DTOs.Responses;

namespace StudentService.Application.Interfaces;

public interface IStudentProfileService
{
    Task CreateSkeletonAsync(CreateStudentProfileRequest request, CancellationToken ct = default);
    Task<StudentProfileResponse> GetMyProfileAsync(Guid userId, CancellationToken ct = default);
    Task<StudentProfileResponse> GetProfileByIdAsync(Guid userId, CancellationToken ct = default);
    Task<StudentProfileResponse> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoRequest request, CancellationToken ct = default);
    Task<EducationResponse> AddEducationAsync(Guid userId, AddEducationRequest request, CancellationToken ct = default);
    Task<EducationResponse> UpdateEducationAsync(Guid userId, Guid educationId, UpdateEducationRequest request, CancellationToken ct = default);
    Task DeleteEducationAsync(Guid userId, Guid educationId, CancellationToken ct = default);
    Task<List<string>> ReplaceSkillsAsync(Guid userId, ReplaceSkillsRequest request, CancellationToken ct = default);
    Task<ProjectResponse> AddProjectAsync(Guid userId, AddProjectRequest request, CancellationToken ct = default);
    Task<ProjectResponse> UpdateProjectAsync(Guid userId, Guid projectId, UpdateProjectRequest request, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken ct = default);
    Task<CertificationResponse> AddCertificationAsync(Guid userId, AddCertificationRequest request, CancellationToken ct = default);
    Task<CertificationResponse> UpdateCertificationAsync(Guid userId, Guid certId, UpdateCertificationRequest request, CancellationToken ct = default);
    Task DeleteCertificationAsync(Guid userId, Guid certId, CancellationToken ct = default);
    Task<SharedKernel.Models.PagedResult<StudentListItemResponse>> GetStudentsByCollegeAsync(Guid collegeId, int page, int pageSize, string? search, CancellationToken ct = default);
    Task<StudentEligibilityResponse> GetEligibilityAsync(Guid userId, CancellationToken ct = default);
    Task<StudentSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken ct = default);
}