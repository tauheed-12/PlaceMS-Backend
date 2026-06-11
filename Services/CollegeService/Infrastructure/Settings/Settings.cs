namespace CollegeService.Infrastructure.Settings;

public class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}

public class KafkaSettings
{
    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "college-service";
}

public class ServiceUrlSettings
{
    public string IdentityService { get; init; } = string.Empty;
}