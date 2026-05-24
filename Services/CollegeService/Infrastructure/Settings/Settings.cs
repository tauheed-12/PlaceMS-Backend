namespace CollegeService.Infrastructure.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "college-service";
}

public class ServiceUrlSettings
{
    public string IdentityService { get; init; } = string.Empty;
}