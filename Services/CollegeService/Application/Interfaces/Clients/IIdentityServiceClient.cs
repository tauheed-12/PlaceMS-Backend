using CollegeService.Application.DTOs.Requests;
using SharedKernel.Enums;


namespace CollegeService.Application.Interfaces.Clients;

public interface IIdentityServiceClient
{
    Task<TpoRegistrationResult?> RegisterTpoAsync(RegisterTpoIdentityRequestDto request, CancellationToken ct);
    Task<TpoDetails?> GetTpoDetails(Guid tpoId, CancellationToken ct);
    Task<List<TpoDetails>?> GetTpoDetailsByIdsBatchAsync(IEnumerable<Guid> tpoIds, CancellationToken ct);
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
    public AccountStatus AccountStatus { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeCode { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}