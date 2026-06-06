using SharedKernel.Abstractions;

namespace DriveService.Domain.Events;

public record DriveCreatedDomainEvent(
    Guid DriveId,
    List<string> CollegeName,
    List<string> CollegeCode,
    string CompanyName,
    string Role) : BaseDomainEvent
{
    public override string EventType => "drive.created";
}

