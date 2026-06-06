namespace DriveService.Infrastructure.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "drive-service";
}
