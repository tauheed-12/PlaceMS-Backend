using CollegeService.Application.Interfaces;
using CollegeService.Domain.Events;
using SharedKernel.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace CollegeService.Infrastructure.Kafka;

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
            case CollegeCreatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.CollegeRegistered,
                    new MessageEnvelope<CollegeRegisteredEvent>
                    {
                        Source = "CollegeService",
                        Topic = KafkaTopics.CollegeRegistered,
                        EventType = e.EventType,
                        Payload = new CollegeRegisteredEvent
                        {
                            CollegeId = e.CollegeId,
                            CollegeName = e.CollegeName,
                            CollegeCode = e.CollegeCode,
                            RegisteredByAdminId = e.CreatedByName
                        }
                    }, ct);
                break;

            case CollegeActivatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.CollegeActivated,
                    new MessageEnvelope<CollegeActivatedEvent>
                    {
                        Source = "CollegeService",
                        Topic = KafkaTopics.CollegeActivated,
                        EventType = e.EventType,
                        Payload = new CollegeActivatedEvent
                        {
                            CollegeId = e.CollegeId,
                            CollegeName = e.CollegeName,
                            CollegeCode = e.CollegeCode,
                            ByAdminId = e.ActivatedBy
                        }
                    }, ct);
                break;

            case CollegeDeactivatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.CollegeDeactivated,
                    new MessageEnvelope<CollegeDeactivatedEvent>
                    {
                        Source = "CollegeService",
                        Topic = KafkaTopics.CollegeDeactivated,
                        EventType = e.EventType,
                        Payload = new CollegeDeactivatedEvent
                        {
                            CollegeId = e.CollegeId,
                            CollegeName = e.CollegeName,
                            CollegeCode = e.CollegeCode,
                            ByAdminId = e.DeactivatedBy
                        }
                    }, ct);
                break;

            case TpoAssignedToCollegeDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.TpoAssigned,
                    new MessageEnvelope<TpoAssignedEvent>
                    {
                        Source = "CollegeService",
                        Topic = KafkaTopics.TpoAssigned,
                        EventType = e.EventType,
                        Payload = new TpoAssignedEvent
                        {
                            TpoUserId = e.TpoId,
                            TpoEmail = e.TpoEmail,
                            TpoName = e.TpoName,
                            CollegeId = e.CollegeId,
                            CollegeName = e.CollegeName,
                            CollegeCode = e.CollegeCode
                        }
                    }, ct);
                break;

            default:
                _logger.LogWarning("No handler for domain event type {EventType}", @event.EventType);
                break;
        }
    }
}

