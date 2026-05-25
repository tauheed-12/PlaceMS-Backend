using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using SharedKernel.Enums;

public interface ICollegeTpoService
{
    Task<TpoDetailsDto> AssignPrimaryTpoAsync(Guid collegeId, CreateTpoRequestDto request, Guid assignedBy, CancellationToken ct);
    Task RemoveTpoAsync(Guid collegeId, Guid userId, CancellationToken ct);
    // Task<List<TpoShortDto>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct);
    Task<TpoDetailsDto?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct);
    Task<bool> IsPrimaryTpoAsync(Guid collegeId, Guid userId, CancellationToken ct);
}

public record TpoRegistrationResult
{
    public Guid UserId { get; init; }
}

public record TpoDetails
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public VerificationStatus VerificationStatus { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeCode { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}