using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.Interfaces.Clients;
using SharedKernel.Exceptions;
using SharedKernel.Enums;
using System.Net.Http.Headers;

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

    private async static Task<HttpResponseMessage> SendWithRetriesAsync(Func<Task<HttpResponseMessage>> action, CancellationToken ct)
    {
        const int maxAttempts = 3;
        var delay = 200; // ms

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var resp = await action();
                if ((int)resp.StatusCode >= 500 && attempt < maxAttempts)
                {
                    await Task.Delay(delay, ct);
                    delay *= 2;
                    continue;
                }

                return resp;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(delay, ct);
                delay *= 2;
            }
        }

        // last attempt
        return await action();
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
                Role = UserRole.TPO,
                CollegeId = requestDto.CollegeId,
                CollegeCode = requestDto.CollegeCode
            };

            var response = await SendWithRetriesAsync(() => _httpClient.PostAsJsonAsync("/api/v1/users", request, ct), ct);

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
            var response = await SendWithRetriesAsync(() => _httpClient.GetAsync($"/api/v1/users/{tpoId}", ct), ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogError("Unauthorized to fetch TPO details for user {UserId}. StatusCode: {StatusCode}",
                    tpoId, response.StatusCode);
                return null;
            }

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

        // Limit concurrency to avoid overloading the identity service
        const int maxParallel = 20;
        using var semaphore = new SemaphoreSlim(maxParallel);

        var tasks = tpoIds.Select(async id =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await GetTpoDetails(id, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(t => t is not null).Cast<TpoDetails>().ToList();
    }

    public async Task<UserDetails?> GetUserDetailsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var response = await SendWithRetriesAsync(() => _httpClient.GetAsync($"/api/v1/users/{userId}", ct), ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogError("Unauthorized to fetch user details for user {UserId}. StatusCode: {StatusCode}",
                    userId, response.StatusCode);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UserApiResponse>(cancellationToken: ct);
            if (result?.Data is null) return null;

            return MapUserToUserDetails(result.Data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch user details for user {UserId}", userId);
            throw new ServiceUnavailableException("IdentityService",
                "Unable to fetch user details at this time.");
        }
    }

    public async Task<List<UserDetails>?> GetUserDetailsByIdsBatchAsync(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        if (userIds == null || !userIds.Any())
            return new List<UserDetails>();

        const int maxParallel = 20;
        using var semaphore = new SemaphoreSlim(maxParallel);

        var tasks = userIds.Select(async id =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await GetUserDetailsAsync(id, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(t => t is not null).Cast<UserDetails>().ToList();
    }

    private static TpoDetails MapUserToTpoDetails(UserData user)
    {
        var verificationStatus = VerificationStatus.Unverified;
        if (!Enum.TryParse<VerificationStatus>(user.VerificationStatus, true, out verificationStatus))
        {
            verificationStatus = VerificationStatus.Unverified;
        }

        var accountStatus = AccountStatus.Active;
        if (!Enum.TryParse<AccountStatus>(user.AccountStatus, true, out accountStatus))
        {
            accountStatus = AccountStatus.Active;
        }

        return new TpoDetails
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            CollegeId = user.CollegeId ?? Guid.Empty,
            CollegeCode = user.CollegeCode ?? string.Empty,
            CollegeName = string.Empty,
            VerificationStatus = verificationStatus,
            AccountStatus = accountStatus,
            CreatedAt = user.CreatedAt
        };
    }

    private static UserDetails MapUserToUserDetails(UserData user)
    {
        var verificationStatus = VerificationStatus.Unverified;
        if (!Enum.TryParse<VerificationStatus>(user.VerificationStatus, true, out verificationStatus))
        {
            verificationStatus = VerificationStatus.Unverified;
        }

        var accountStatus = AccountStatus.Active;
        if (!Enum.TryParse<AccountStatus>(user.AccountStatus, true, out accountStatus))
        {
            accountStatus = AccountStatus.Active;
        }

        return new UserDetails
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            VerificationStatus = verificationStatus,
            AccountStatus = accountStatus,
            CollegeId = user.CollegeId,
            CollegeCode = user.CollegeCode ?? string.Empty,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<TpoDetails?> ActivateTpoAccount(Guid tpoId, CancellationToken ct)
    {
        try
        {
            var response = await SendWithRetriesAsync(() => _httpClient.PostAsync($"/api/v1/users/{tpoId}/activate", null, ct), ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UserApiResponse>(cancellationToken: ct);
            if (result?.Data is null) return null;

            return MapUserToTpoDetails(result.Data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to activate TPO account {UserId}", tpoId);
            throw new ServiceUnavailableException("IdentityService", "Unable to activate TPO account at this time.");
        }
    }

    public async Task<TpoDetails?> DeactivateTpoAccount(Guid tpoId, CancellationToken ct)
    {
        try
        {
            var response = await SendWithRetriesAsync(() => _httpClient.PostAsync($"/api/v1/users/{tpoId}/deactivate", null, ct), ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UserApiResponse>(cancellationToken: ct);
            if (result?.Data is null) return null;

            return MapUserToTpoDetails(result.Data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to deactivate TPO account {UserId}", tpoId);
            throw new ServiceUnavailableException("IdentityService", "Unable to deactivate TPO account at this time.");
        }
    }

    private record RegisterUserRequest
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public UserRole Role { get; init; }
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
        public string Role { get; init; } = string.Empty;
        public string? CollegeCode { get; init; }
        public Guid? CollegeId { get; init; }
        public string VerificationStatus { get; init; } = string.Empty;
        public string AccountStatus { get; init; } = string.Empty;
        public DateTime? LastLoginAt { get; init; }
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