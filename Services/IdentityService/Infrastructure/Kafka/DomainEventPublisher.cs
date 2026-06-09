using IdentityService.Application.Interfaces;
using IdentityService.Domain.Events;
using Microsoft.Extensions.Logging;
using SharedKernel.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace IdentityService.Infrastructure.Kafka;

/// <summary>
/// Reads domain events accumulated on aggregates and publishes
/// them as Kafka messages. Called after SaveChanges succeeds.
/// Each event type maps to a Kafka topic defined in KafkaTopics constants.
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IEventPublisher _kafkaPublisher;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(IEventPublisher kafkaPublisher, ILogger<DomainEventPublisher> logger)
    {
        _kafkaPublisher = kafkaPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var @event in events)
        {
            try
            {
                await DispatchAsync(@event, ct);
            }
            catch (Exception ex)
            {
                // Log but don't throw — domain event publishing failure
                // should not roll back the already-committed transaction.
                // In production, use an outbox pattern for guaranteed delivery.
                _logger.LogError(ex,
                    "Failed to publish domain event {EventType} with ID {EventId}",
                    @event.EventType, @event.EventId);
            }
        }
    }

    private async Task DispatchAsync(IDomainEvent @event, CancellationToken ct)
    {
        switch (@event)
        {
            case UserCreatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.UserRegistered,
                    new MessageEnvelope<UserRegisteredEvent>
                    {
                        Source = "IdentityService",
                        Topic = KafkaTopics.UserRegistered,
                        EventType = e.EventType,
                        Payload = new UserRegisteredEvent
                        {
                            UserId = e.UserId,
                            Email = e.Email,
                            FullName = e.FullName,
                            Role = e.Role,
                            // Token is retrieved separately by the email service
                            VerificationToken = e.EmailVerificationToken,
                            VerificationLink = e.EmailVerificationLink
                        }
                    }, ct);
                break;

            case UserEmailVerificationDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.UserEmailVerification,
                    new MessageEnvelope<UserEmailVerificationEvent>
                    {
                        Source = "IdentityService",
                        Topic = KafkaTopics.UserEmailVerification,
                        EventType = "user.email-verification",
                        Payload = new UserEmailVerificationEvent
                        {
                            UserId = e.UserId,
                            Email = e.Email,
                            FullName = e.FullName,
                            VerificationToken = string.Empty,
                            VerificationLink = string.Empty
                        }
                    }, ct);
                break;

            case UserEmailVerifiedDomainEvent e:
                _logger.LogInformation("Email verified for user {UserId}", e.UserId);
                break;

            case UserDeactivatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.UserDeactivated,
                    new MessageEnvelope<object>
                    {
                        Source = "IdentityService",
                        Topic = KafkaTopics.UserDeactivated,
                        EventType = e.EventType,
                        Payload = new { e.UserId, e.Email, e.DeactivatedBy }
                    }, ct);
                break;

            default:
                _logger.LogWarning("No handler for domain event type {EventType}", @event.EventType);
                break;
        }
    }
}