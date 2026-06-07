using SharedKernel.Abstractions;

namespace ApplicationService.Domain.Events;

public record ApplicationCreatedDomainEvent(
    Guid ApplicationId,
    Guid DriveId,
    Guid CollegeId,
    Guid StudentId,
    string StudentEmail) : BaseDomainEvent
{
    public override string EventType => "application.created";
}

public record ApplicationStatusUpdatedDomainEvent(
    Guid ApplicationId,
    Guid DriveId,
    Guid CollegeId,
    Guid StudentId,
    string StudentEmail,
    string OldStatus,
    string NewStatus) : BaseDomainEvent
{
    public override string EventType => "application.status_updated";
}

public record ApplicationWithdrawnDomainEvent(
    Guid ApplicationId,
    Guid DriveId,
    Guid CollegeId,
    Guid StudentId,
    string StudentEmail) : BaseDomainEvent
{
    public override string EventType => "application.withdrawn";
}