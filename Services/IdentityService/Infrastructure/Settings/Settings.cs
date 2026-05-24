namespace IdentityService.Infrastructure.Settings;

public class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; init; } = 15;
    public int RefreshTokenExpiryDays { get; init; } = 7;
}

public class KafkaSettings
{
    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "identity-service";
}

public class ServiceUrlSettings
{
    public string CollegeService { get; init; } = string.Empty;
    public string NotificationService { get; init; } = string.Empty;
}