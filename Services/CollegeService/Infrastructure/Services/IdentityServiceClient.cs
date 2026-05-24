using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.Interfaces;
using SharedKernel.Exceptions;

namespace CollegeService.Infrastructure.Services;

public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityServiceClient> _logger;

    public IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TpoRegistrationResult?> RegisterTpoAsync(RegisterTpoIdentityRequestDto requestDto, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/register/tpo", requestDto, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Failed to register TPO in IdentityService. StatusCode: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    error);

                return null;
            }
            var result = await response.Content
                .ReadFromJsonAsync<TpoRegistrationApiResponse>(cancellationToken: ct);

            if (result?.Data is null) return null;

            return new TpoRegistrationResult
            {
                UserId = result.Data.UserId
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach IdentityService for tpo {FullName}", requestDto.FullName);
            throw new ServiceUnavailableException("IdentityService",
                "Unable to register tpo at this time.");
        }
    }

    private record TpoRegistrationApiResponse
    {
        public bool Success { get; init; }
        public TpoRegistrationData? Data { get; init; }
    }

    private record TpoRegistrationData
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Message { get; init; } = "Registration successful. Please check your email to verify your account.";
    }
}