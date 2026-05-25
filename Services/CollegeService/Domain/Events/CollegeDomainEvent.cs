using SharedKernel.Abstractions;

namespace CollegeService.Domain.Events;


// CollegeDomanEvent
public record CollegeCreatedDomainEvent(
    Guid CollegeId,
    string CollegeName,
    string CollegeCode,
    Guid CreatedByName)
    : BaseDomainEvent
{
    public override string EventType => "college.created";
}

public record CollegeActivatedDomainEvent(
    Guid CollegeId,
    string CollegeName,
    string CollegeCode,
    Guid ActivatedBy)
    : BaseDomainEvent
{
    public override string EventType => "college.activated";
}

public record CollegeDeactivatedDomainEvent(
    Guid CollegeId,
    string CollegeName,
    string CollegeCode,
    Guid DeactivatedBy)
    : BaseDomainEvent
{
    public override string EventType => "college.deactivated";
}

public record TpoAssignedToCollegeDomainEvent(
    Guid TpoId,
    Guid CollegeId,
    string CollegeName,
    Guid AssignedBy)
    : BaseDomainEvent
{
    public override string EventType => "college.tpo.assigned";
}

// TPO Domain Events
public record TpoRemovedFromCollegeDomainEvent(
    Guid TpoId,
    Guid CollegeId,
    string CollegeName,
    Guid RemovedBy)
    : BaseDomainEvent
{
    public override string EventType => "college.tpo.removed";
}

public record TpoActivatedDomainEvent(
    Guid TpoId,
    Guid CollegeId,
    Guid ActivatedBy)
    : BaseDomainEvent
{
    public override string EventType => "tpo.activated";
}

public record TpoDeactivatedDomainEvent(
    Guid TpoId,
    Guid CollegeId,
    Guid DeactivatedBy)
    : BaseDomainEvent
{
    public override string EventType => "tpo.deactivated";
}


// Admin domain events
public record CollegeAssignedToAdminDomainEvent(
    Guid AdminId,
    Guid CollegeId,
    string CollegeName,
    string CollegeCode,
    Guid AssignedBy)
    : BaseDomainEvent
{
    public override string EventType => "admin.college.assigned";
}

public record CollegeUnassignedFromAdminDomainEvent(
    Guid AdminId,
    Guid CollegeId,
    string CollegeName,
    string CollegeCode,
    Guid UnassignedBy)
    : BaseDomainEvent
{
    public override string EventType => "admin.college.unassigned";
}