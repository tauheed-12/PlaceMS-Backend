using DriveService.Application.Interfaces;
using DriveService.Domain.Events;
using DriveService.Infrastructure.Settings;
using SharedKernel.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Messaging;

namespace DriveService.Infrastructure.Kafka;

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
            case DriveCreatedDomainEvent e:
                foreach (var college in e.Colleges)
                {
                    await _kafkaPublisher.PublishAsync(
                        KafkaTopics.DriveApprovalRequested,
                        new MessageEnvelope<DriveApprovalRequestedEvent>
                        {
                            Source = "DriveService",
                            Topic = KafkaTopics.DriveApprovalRequested,
                            EventType = e.EventType,
                            Payload = new DriveApprovalRequestedEvent
                            {
                                DriveId = e.DriveId,
                                CompanyName = e.CompanyName,
                                JobRole = e.JobRole,
                                CollegeId = college.CollegeId,
                                CollegeName = college.CollegeName,
                                TpoUserId = college.TpoUserId ?? Guid.Empty,
                                TpoEmail = college.TpoEmail ?? string.Empty,
                                TpoName = college.TpoName ?? string.Empty,
                                DriveDeadline = e.ApplicationDeadline.ToString("o")
                            }
                        }, ct);
                }
                break;

            case DriveApprovedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.DriveApproved,
                    new MessageEnvelope<DriveApprovedEvent>
                    {
                        Source = "DriveService",
                        Topic = KafkaTopics.DriveApproved,
                        EventType = e.EventType,
                        Payload = new DriveApprovedEvent
                        {
                            DriveId = e.DriveId,
                            CompanyName = e.CompanyName,
                            JobRole = e.JobRole,
                            CollegeId = e.CollegeId,
                            TpoUserId = e.TpoUserId,
                            Note = e.Note ?? string.Empty
                        }
                    }, ct);
                break;

            case DriveRejectedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.DriveRejected,
                    new MessageEnvelope<DriveRejectedEvent>
                    {
                        Source = "DriveService",
                        Topic = KafkaTopics.DriveRejected,
                        EventType = e.EventType,
                        Payload = new DriveRejectedEvent
                        {
                            DriveId = e.DriveId,
                            CompanyName = e.CompanyName,
                            CollegeId = e.CollegeId,
                            TpoUserId = e.TpoUserId,
                            RejectionNote = e.Note,
                            RecruiterUserId = e.RecruiterUserId,
                            RecruiterEmail = string.Empty
                        }
                    }, ct);
                break;

            case DriveChangesRequestedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.DriveChangesRequested,
                    new MessageEnvelope<DriveChangesRequestedEvent>
                    {
                        Source = "DriveService",
                        Topic = KafkaTopics.DriveChangesRequested,
                        EventType = e.EventType,
                        Payload = new DriveChangesRequestedEvent
                        {
                            DriveId = e.DriveId,
                            CompanyName = e.CompanyName,
                            JobRole = e.JobRole,
                            CollegeId = e.CollegeId,
                            CollegeName = e.CollegeName,
                            TpoUserId = e.TpoUserId,
                            TpoName = string.Empty,
                            ChangeNote = e.Note,
                            RecruiterUserId = e.RecruiterUserId,
                            RecruiterEmail = string.Empty
                        }
                    }, ct);
                break;

            case DriveResubmittedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.DriveResubmitted,
                    new MessageEnvelope<DriveResubmittedEvent>
                    {
                        Source = "DriveService",
                        Topic = KafkaTopics.DriveResubmitted,
                        EventType = e.EventType,
                        Payload = new DriveResubmittedEvent
                        {
                            DriveId = e.DriveId,
                            CompanyName = e.CompanyName,
                            JobRole = e.JobRole,
                            CollegeId = e.CollegeId,
                            CollegeName = e.CollegeName,
                            TpoUserId = e.TpoUserId ?? Guid.Empty,
                            TpoEmail = string.Empty,
                            TpoName = string.Empty,
                            RecruiterUserId = e.RecruiterUserId,
                            RecruiterEmail = string.Empty
                        }
                    }, ct);
                break;

            case DriveDeactivatedDomainEvent e:
                await _kafkaPublisher.PublishAsync(
                    KafkaTopics.DriveDeactivated,
                    new MessageEnvelope<DriveDeactivatedEvent>
                    {
                        Source = "DriveService",
                        Topic = KafkaTopics.DriveDeactivated,
                        EventType = e.EventType,
                        Payload = new DriveDeactivatedEvent
                        {
                            DriveId = e.DriveId,
                            CompanyName = e.CompanyName,
                            JobRole = e.JobRole,
                            RecruiterUserId = e.RecruiterUserId,
                            RecruiterEmail = string.Empty,
                            CollegeIds = e.CollegeIds
                        }
                    }, ct);
                break;

            default:
                _logger.LogWarning("No handler for domain event type {EventType}", @event.EventType);
                break;
        }
    }
}
