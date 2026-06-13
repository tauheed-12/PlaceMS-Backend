using IdentityService.Application.Interfaces;
using SharedKernel.Exceptions;
using SharedKernel.Enums;

namespace IdentityService.Infrastructure.Services;

public class CollegeServiceClient : ICollegeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CollegeServiceClient> _logger;

    public CollegeServiceClient(HttpClient httpClient, IConfiguration config, ILogger<CollegeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CollegeValidationResult?> ValidateCollegeCodeAsync(string collegeCode, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/internal/colleges/validate/{collegeCode.ToUpperInvariant()}");

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CollegeValidationApiResponse>(
                cancellationToken: ct);

            if (result?.Data is null) return null;

            return new CollegeValidationResult
            {
                CollegeId = result.Data.CollegeId,
                CollegeName = result.Data.CollegeName,
                CollegeCode = result.Data.CollegeCode,
                IsActive = result.Data.IsActive,
                IsValid = result.Data.IsValid,
                AccountStatus = result.Data.AccountStatus
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach CollegeService for code {CollegeCode}", collegeCode);
            throw new ServiceUnavailableException("CollegeService",
                "Unable to validate college code at this time.");
        }
    }

    // Internal DTO matching CollegeService API response shape
    private record CollegeValidationApiResponse
    {
        public bool Success { get; init; }
        public CollegeValidationData? Data { get; init; }
    }

    private record CollegeValidationData
    {
        public bool IsValid { get; init; }
        public AccountStatus AccountStatus { get; init; }
        public Guid CollegeId { get; init; }
        public string CollegeName { get; init; } = string.Empty;
        public string CollegeCode { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }
}