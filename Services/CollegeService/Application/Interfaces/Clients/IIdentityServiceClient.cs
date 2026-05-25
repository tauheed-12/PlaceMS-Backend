using CollegeService.Application.DTOs.Requests;

public interface IIdentityServiceClient
{
    Task<TpoRegistrationResult?> RegisterTpoAsync(RegisterTpoIdentityRequestDto request, CancellationToken ct);
    Task<TpoDetails?> GetTpoDetails(Guid tpoId, CancellationToken ct);
    Task<List<TpoDetails>?> GetTpoDetailsByIdsBatchAsync(List<Guid> tpoIds, CancellationToken ct);
}