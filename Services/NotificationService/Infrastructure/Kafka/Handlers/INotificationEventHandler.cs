namespace NotificationService.Infrastructure.Kafka.Handlers;

public interface INotificationEventHandler
{
    string Topic { get; }
    bool CanHandle(string topic) => Topic == topic;
    Task HandleAsync(string messageValue, CancellationToken ct = default);
}