using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Kafka.Handlers;
using NotificationService.Infrastructure.Settings;
using SharedKernel.Constants;

namespace NotificationService.Infrastructure.Kafka.Consumers;

/// <summary>
/// Single consumer that subscribes to ALL notification-relevant topics.
/// Routes each message to the appropriate handler based on topic name.
/// Adding a new event = register a new INotificationEventHandler.
/// </summary>
public class NotificationKafkaConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaSettings _settings;
    private readonly ILogger<NotificationKafkaConsumer> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<string> _topics = new()
    {
        KafkaTopics.UserEmailVerification,
        KafkaTopics.UserPasswordReset,
        KafkaTopics.UserRegistered,
        KafkaTopics.TpoAssigned,
        KafkaTopics.CoordinatorAdded,
        KafkaTopics.DriveApprovalRequested,
        KafkaTopics.DriveApproved,
        KafkaTopics.DriveRejected,
        KafkaTopics.ApplicationSubmitted,
        KafkaTopics.ApplicationStatusChanged,
        KafkaTopics.PlacementConfirmed
    };

    public NotificationKafkaConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<NotificationKafkaConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationKafkaConsumer starting. Topics: {Topics}",
            string.Join(", ", _topics));

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            MaxPollIntervalMs = 300000,
            SessionTimeoutMs = 45000
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_topics);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result is null) continue;

                _logger.LogDebug("Received message on topic {Topic}", result.Topic);

                await ProcessAsync(result.Topic, result.Message.Value, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in NotificationKafkaConsumer");
                await Task.Delay(5000, stoppingToken);
            }
        }

        consumer.Close();
        _logger.LogInformation("NotificationKafkaConsumer stopped.");
    }

    private async Task ProcessAsync(string topic, string messageValue, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider
            .GetServices<INotificationEventHandler>()
            .ToList();

        var handler = handlers.FirstOrDefault(h => h.CanHandle(topic));

        if (handler is null)
        {
            _logger.LogWarning("No handler registered for topic {Topic}", topic);
            return;
        }

        await handler.HandleAsync(messageValue, ct);
    }
}