namespace ApplicationService.Infrastructure.Settings;

public class ServiceUrlSettings
{
    public string IdentityService { get; init; } = string.Empty;
    public string StudentService { get; init; } = string.Empty;
    public string DriveService { get; init; } = string.Empty;
}

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