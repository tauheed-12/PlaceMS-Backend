using SharedKernel.Abstractions;

namespace StudentService.Domain.Events;

public record StudentProfileCreatedDomainEvent(
    Guid UserId,
    string Email,
    Guid CollegeId) : BaseDomainEvent
{
    public override string EventType => "student.profile-created";
}

public record StudentProfileUpdatedDomainEvent(
    Guid UserId,
    int NewCompletionScore) : BaseDomainEvent
{
    public override string EventType => "student.profile-updated";
}

public record ResumeUploadedDomainEvent(
    Guid UserId,
    Guid ResumeFileId,
    string FileName) : BaseDomainEvent
{
    public override string EventType => "student.resume-uploaded";
}