
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
            case CollegeRegisterDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.CollegeRegistered,
                    new MessageEnvelope<CollegeRegisteredEvent>
                    {
                        Source = "CollegeService",
                        Topic = KafkaTopics.CollegeRegistered,
                        EventType = e.EventType,
                        Payload = new CollegeRegisteredEvent
                        {
                            CollegeId = e.Id,
                            CollegeEmail = e.Email,
                            CollegeCode = e.Code,
                            RegisteredByAdminId = e.RegisteredBy
                        }
                    }, ct);
                break;

            default:
                _logger.LogWarning("No handler for domain event type {EventType}", @event.EventType);
                break;
        }
    }
}

