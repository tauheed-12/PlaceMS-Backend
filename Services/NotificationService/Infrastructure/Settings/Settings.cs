namespace NotificationService.Infrastructure.Settings;

public class SmtpSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "PMS Notifications";
    public bool EnableSsl { get; init; } = true;
}

public class KafkaSettings
{
    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "notification-service";
}