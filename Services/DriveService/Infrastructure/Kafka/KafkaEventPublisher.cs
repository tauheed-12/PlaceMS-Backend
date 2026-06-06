using System.Text.Json;
using Confluent.Kafka;
using DriveService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using SharedKernel.Abstractions;

namespace DriveService.Infrastructure.Kafka;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public KafkaEventPublisher(IOptions<KafkaSettings> settings, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        var payload = JsonSerializer.Serialize(@event, _jsonOptions);
        var message = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = payload
        };

        var result = await _producer.ProduceAsync(topic, message, ct);

        _logger.LogDebug(
            "Published event to Kafka. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
            result.Topic,
            result.Partition.Value,
            result.Offset.Value);
    }

    public void Dispose() => _producer?.Dispose();
}
