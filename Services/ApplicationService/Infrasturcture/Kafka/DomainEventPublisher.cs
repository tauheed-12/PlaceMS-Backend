using ApplicationService.Application.Interfaces;
using ApplicationService.Domain.Events;
using ApplicationService.Infrastructure.Settings;
using SharedKernel.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace ApplicationService.Infrastructure.Kafka;

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
                    @event.EventType,
                    @event.EventId);
            }
        }
    }

    private async Task DispatchAsync(IDomainEvent @event, CancellationToken ct)
    {
        switch (@event)
        {
            case ApplicationCreatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                        KafkaTopics.ApplicationSubmitted,
                        new MessageEnvelope<ApplicationSubmittedEvent>
                        {
                            Source = "ApplicationService",
                            Topic = KafkaTopics.ApplicationSubmitted,
                            EventType = e.EventType,
                            Payload = new ApplicationSubmittedEvent
                            {
                                ApplicationId = e.ApplicationId,
                                StudentUserId = e.StudentId,
                                StudentName = e.StudentName,
                                StudentEmail = e.StudentEmail,
                                DriveId = e.DriveId,
                                CompanyName = e.CompanyName,
                                JobRole = e.JobRole
                            }
                        }, ct);
                break;

            case ApplicationStatusUpdatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                        KafkaTopics.ApplicationStatusChanged,
                        new MessageEnvelope<ApplicationStatusChangedEvent>
                        {
                            Source = "ApplicationService",
                            Topic = KafkaTopics.ApplicationStatusChanged,
                            EventType = e.EventType,
                            Payload = new ApplicationStatusChangedEvent
                            {
                                ApplicationId = e.ApplicationId,
                                StudentUserId = e.StudentId,
                                DriveId = e.DriveId,
                                StudentName = e.StudentName,
                                StudentEmail = e.StudentEmail,
                                CompanyName = e.CompanyName,
                                JobRole = e.JobRole,
                                PreviousStatus = e.OldStatus,
                                NewStatus = e.NewStatus
                            }
                        }, ct);
                break;

            case ApplicationWithdrawnDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                        KafkaTopics.ApplicationWithdrawn,
                        new MessageEnvelope<ApplicationWithdrawnEvent>
                        {
                            Source = "ApplicationService",
                            Topic = KafkaTopics.ApplicationWithdrawn,
                            EventType = e.EventType,
                            Payload = new ApplicationWithdrawnEvent
                            {
                                ApplicationId = e.ApplicationId,
                                StudentUserId = e.StudentId,
                                StudentName = e.StudentName,
                                StudentEmail = e.StudentEmail,
                                DriveId = e.DriveId,
                                CompanyName = e.CompanyName,
                                JobRole = e.JobRole
                            }
                        }, ct);
                break;

            default:
                _logger.LogWarning("No handler for domain event type {EventType}", @event.EventType);
                break;
        }
    }
}
