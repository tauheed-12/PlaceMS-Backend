namespace DriveService.Infrastructure.Services;

/// <summary>
/// OAuth 2.0 Client Credentials Flow Token Provider for service-to-service authentication.
/// 
/// This provider handles obtaining JWT tokens from IdentityService using client credentials.
/// Tokens are cached to reduce network calls, with automatic refresh when expiry approaches.
/// </summary>
public interface IServiceTokenProvider
{
    Task<string> GetServiceTokenAsync(CancellationToken ct);
}

public class ServiceTokenProvider : IServiceTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<ServiceTokenProvider> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public ServiceTokenProvider(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<ServiceTokenProvider> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Gets a valid service token, using cache when available.
    /// Automatically refreshes token if it's expiring within 5 minutes.
    /// </summary>
    public async Task<string> GetServiceTokenAsync(CancellationToken ct)
    {
        // Return cached token if still valid (with 5-minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            _logger.LogDebug("Using cached service token");
            return _cachedToken;
        }

        // Get new token using OAuth 2.0 Client Credentials flow
        var clientId = _config["ServiceCredentials:ClientId"];
        var clientSecret = _config["ServiceCredentials:ClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException(
                "Service credentials not configured. Set ServiceCredentials:ClientId and ServiceCredentials:ClientSecret in appsettings.");
        }

        var request = new
        {
            clientId,
            clientSecret
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/auth/client-credentials",
                request,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Failed to obtain service token from IdentityService. StatusCode: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    error);
                throw new InvalidOperationException(
                    $"Failed to authenticate with IdentityService: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ServiceTokenApiResponse>(
                cancellationToken: ct);

            if (result?.Data is null || string.IsNullOrWhiteSpace(result.Data.AccessToken))
            {
                throw new InvalidOperationException("No token received from IdentityService");
            }

            _cachedToken = result.Data.AccessToken;
            // Store expiry with buffer (expires_in is in seconds)
            _tokenExpiry = DateTime.UtcNow.AddSeconds(result.Data.ExpiresIn - 60);

            _logger.LogInformation("Service token obtained successfully, expires in {ExpiresIn} seconds", result.Data.ExpiresIn);

            return _cachedToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach IdentityService for service token");
            throw new InvalidOperationException(
                "Unable to authenticate with IdentityService. The service may be unavailable.");
        }
    }

    // Internal response types for deserialization
    private record ServiceTokenApiResponse
    {
        public ServiceTokenData? Data { get; init; }
    }

    private record ServiceTokenData
    {
        public string AccessToken { get; init; } = string.Empty;
        public string TokenType { get; init; } = "Bearer";
        public int ExpiresIn { get; init; }
    }
}
