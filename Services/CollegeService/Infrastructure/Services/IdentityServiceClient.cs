using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.Interfaces.Clients;
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
            var request = new RegisterUserRequest
            {
                FullName = requestDto.FullName,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                Password = Guid.NewGuid().ToString("N"),
                Role = requestDto.Role,
                CollegeId = requestDto.CollegeId,
                CollegeCode = requestDto.CollegeCode
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/users", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Failed to register TPO in IdentityService. StatusCode: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    error);

                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TpoRegistrationApiResponse>(cancellationToken: ct);
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

    public async Task<TpoDetails?> GetTpoDetails(Guid tpoId, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{tpoId}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UserApiResponse>(cancellationToken: ct);
            if (result?.Data is null) return null;

            return MapUserToTpoDetails(result.Data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch TPO details for user {UserId}", tpoId);
            throw new ServiceUnavailableException("IdentityService",
                "Unable to fetch TPO details at this time.");
        }
    }

    public async Task<List<TpoDetails>?> GetTpoDetailsByIdsBatchAsync(IEnumerable<Guid> tpoIds, CancellationToken ct)
    {
        if (tpoIds == null || !tpoIds.Any())
            return new List<TpoDetails>();

        var tasks = tpoIds.Select(id => GetTpoDetails(id, ct));
        var results = await Task.WhenAll(tasks);
        return results.Where(t => t is not null).Cast<TpoDetails>().ToList();
    }

    private static TpoDetails MapUserToTpoDetails(UserData user)
        => new()
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            CollegeId = user.CollegeId ?? Guid.Empty,
            CollegeCode = user.CollegeCode ?? string.Empty,
            CollegeName = string.Empty,
            VerificationStatus = Enum.Parse<SharedKernel.Enums.VerificationStatus>(user.VerificationStatus),
            CreatedAt = user.CreatedAt
        };

    private record RegisterUserRequest
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public SharedKernel.Enums.UserRole Role { get; init; }
        public Guid? CollegeId { get; init; }
        public string? CollegeCode { get; init; }
    }

    private record UserApiResponse
    {
        public bool Success { get; init; }
        public UserData? Data { get; init; }
    }

    private record UserData
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? CollegeCode { get; init; }
        public Guid? CollegeId { get; init; }
        public string VerificationStatus { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
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