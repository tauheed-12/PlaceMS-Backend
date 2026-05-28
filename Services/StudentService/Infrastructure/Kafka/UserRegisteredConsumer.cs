using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Enums;
using SharedKernel.Messaging;
using StudentService.Application.DTOs.Requests;
using StudentService.Application.Interfaces;
using StudentService.Infrastructure.Settings;

namespace StudentService.Infrastructure.Kafka;

/// <summary>
/// Background service that consumes pms.user.registered events.
/// When a new student registers in Identity Service, this consumer
/// creates their skeleton profile in Student Service automatically.
/// </summary>
public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaSettings _settings;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserRegisteredConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<UserRegisteredConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserRegisteredConsumer starting...");

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,          // Manual commit — process then commit
            MaxPollIntervalMs = 300000,
            SessionTimeoutMs = 45000
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(KafkaTopics.UserRegistered);

        _logger.LogInformation("Subscribed to topic: {Topic}", KafkaTopics.UserRegistered);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result is null) continue;

                await ProcessMessageAsync(result.Message.Value, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UserRegisteredConsumer");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        consumer.Close();
        _logger.LogInformation("UserRegisteredConsumer stopped.");
    }

    private async Task ProcessMessageAsync(string messageValue, CancellationToken ct)
    {
        MessageEnvelope<UserRegisteredEvent>? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<MessageEnvelope<UserRegisteredEvent>>(
                messageValue, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize UserRegistered message: {Message}", messageValue);
            return; // Skip malformed messages — don't retry
        }

        if (envelope?.Payload is null)
        {
            _logger.LogWarning("Received empty or null payload in UserRegistered message.");
            return;
        }

        var payload = envelope.Payload;

        // Only process Student registrations
        if (!string.Equals(payload.Role, UserRole.Student.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Skipping UserRegistered event for role {Role} — not a student.", payload.Role);
            return;
        }

        _logger.LogInformation(
            "Processing UserRegistered for student {UserId} | CorrelationId: {CorrelationId}",
            payload.UserId, envelope.CorrelationId);

        using var scope = _scopeFactory.CreateScope();
        var profileService = scope.ServiceProvider.GetRequiredService<IStudentProfileService>();

        await profileService.CreateSkeletonAsync(new CreateStudentProfileRequest
        {
            UserId = payload.UserId,
            FullName = payload.FullName,
            Email = payload.Email,
            PhoneNumber = string.Empty,         // Not in UserRegisteredEvent — student fills later
            CollegeId = Guid.Empty,             // Will be updated when Identity publishes college info
            CollegeCode = string.Empty,
            CollegeName = string.Empty
        }, ct);

        _logger.LogInformation("Skeleton profile created for student {UserId}", payload.UserId);
    }
}