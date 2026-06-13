using IdentityService.Application.DTOs.Requests;
using IdentityService.Application.Interfaces;
namespace IdentityService.Infrastructure.Services;

public interface IServiceTokenProvider
{
    Task<string> GetServiceTokenAsync(CancellationToken ct);
}

public class ServiceTokenProvider : IServiceTokenProvider
{
    private readonly IConfiguration _config;
    private readonly IAuthService _authService;
    private readonly ILogger<ServiceTokenProvider> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public ServiceTokenProvider(IConfiguration config, ILogger<ServiceTokenProvider> logger, IAuthService authService)
    {
        _config = config;
        _logger = logger;
        _authService = authService;
    }

    public async Task<string> GetServiceTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            _logger.LogDebug("Using cached service token");
            return _cachedToken;
        }

        var clientConfig = _config.GetSection("ServiceClients")
            .GetChildren()
            .FirstOrDefault(c => string.Equals(c["ClientId"], "identity-service", StringComparison.OrdinalIgnoreCase));

        if (clientConfig is null)
        {
            throw new InvalidOperationException(
                "Identity service credentials not configured. Set ServiceClients:IdentityClient:ClientId and ServiceClients:IdentityClient:ClientSecret in appsettings.");
        }

        var clientId = clientConfig["ClientId"];
        var clientSecret = clientConfig["ClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException(
                "Identity service credentials not configured. Set ServiceClients:IdentityClient:ClientId and ServiceClients:IdentityClient:ClientSecret in appsettings.");
        }

        var request = new ClientCredentialsRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        var result = await _authService.GetServiceTokenAsync(request, ct);

        _cachedToken = result.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 60);

        _logger.LogInformation("Service token obtained successfully, expires in {ExpiresIn} seconds", result.ExpiresIn);

        return _cachedToken;
    }

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
