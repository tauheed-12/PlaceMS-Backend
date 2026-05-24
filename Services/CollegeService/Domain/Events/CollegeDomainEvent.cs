using SharedKernel.Abstractions;

namespace CollegeService.Domain.Events;

public record CollegeRegisterDomainEvent(Guid Id, string Email, string Name, string Code, string RegisteredBy) : BaseDomainEvent
{
    public override string EventType => "college.created";
}

public record CollegeDeactivatedDomainEvent(Guid UserId, string Email, string DeactivatedBy) : BaseDomainEvent
{
    public override string EventType => "user.deactivated";
}