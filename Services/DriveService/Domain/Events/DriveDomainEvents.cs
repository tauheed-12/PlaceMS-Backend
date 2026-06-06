using SharedKernel.Abstractions;
using SharedKernel.Enums;

namespace DriveService.Domain.Events;

public record DriveCollegeInfo(
    Guid CollegeId,
    string CollegeName,
    Guid? TpoUserId,
    string? TpoEmail,
    string? TpoName);

public record DriveCreatedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid RecruiterUserId,
    DateTime ApplicationDeadline,
    List<DriveCollegeInfo> Colleges) : BaseDomainEvent
{
    public override string EventType => "drive.created";
}

public record DriveUpdatedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid RecruiterUserId) : BaseDomainEvent
{
    public override string EventType => "drive.updated";
}

public record DriveApprovedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid CollegeId,
    string CollegeName,
    Guid TpoUserId,
    string? Note) : BaseDomainEvent
{
    public override string EventType => "drive.approved";
}

public record DriveRejectedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid CollegeId,
    string CollegeName,
    Guid TpoUserId,
    string Note,
    Guid RecruiterUserId) : BaseDomainEvent
{
    public override string EventType => "drive.rejected";
}

public record DriveChangesRequestedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid CollegeId,
    string CollegeName,
    Guid TpoUserId,
    string Note,
    Guid RecruiterUserId) : BaseDomainEvent
{
    public override string EventType => "drive.changes_requested";
}

public record DriveResubmittedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid CollegeId,
    string CollegeName,
    Guid? TpoUserId,
    Guid RecruiterUserId) : BaseDomainEvent
{
    public override string EventType => "drive.resubmitted";
}

public record DriveDeactivatedDomainEvent(
    Guid DriveId,
    string CompanyName,
    string JobRole,
    Guid RecruiterUserId,
    List<Guid> CollegeIds) : BaseDomainEvent
{
    public override string EventType => "drive.deactivated";
}

