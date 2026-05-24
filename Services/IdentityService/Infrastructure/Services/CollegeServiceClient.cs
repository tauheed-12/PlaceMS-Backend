using System.Net.Http.Json;
using IdentityService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// HTTP client that calls the College Service to validate college codes
/// during student registration. Uses IHttpClientFactory with Polly
/// retry and circuit breaker policies registered in DI.
/// </summary>
public class CollegeServiceClient : ICollegeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CollegeServiceClient> _logger;

    public CollegeServiceClient(HttpClient httpClient, ILogger<CollegeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CollegeValidationResult?> ValidateCollegeCodeAsync(string collegeCode, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/colleges/validate/{collegeCode.ToUpperInvariant()}", ct);

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
                IsActive = result.Data.IsActive
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
        public Guid CollegeId { get; init; }
        public string CollegeName { get; init; } = string.Empty;
        public string CollegeCode { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }
}