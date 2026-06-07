using SharedKernel.Abstractions;
using SharedKernel.Enums;
using SharedKernel.Exceptions;

namespace ApplicationService.Domain.Entities;

public class StudentApplication : AggregateRoot
{
    public Guid DriveId { get; private set; }
    public Guid CollegeId { get; private set; }
    public Guid StudentId { get; private set; }
    public string StudentName { get; private set; } = string.Empty;
    public string StudentEmail { get; private set; } = string.Empty;
    public string CollegeName { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string JobRole { get; private set; } = string.Empty;
    public ApplicationStatus Status { get; private set; }
    public DateTime AppliedOn { get; private set; }

    private StudentApplication() { }

    public static StudentApplication Create(Guid driveId, Guid collegeId, Guid studentId, string studentName, string studentEmail, string collegeName, string companyName, string jobRole)
    {
        if (driveId == Guid.Empty)
            throw new DomainValidationException("DriveId is required.");

        if (collegeId == Guid.Empty)
            throw new DomainValidationException("CollegeId is required.");

        if (studentId == Guid.Empty)
            throw new DomainValidationException("StudentId is required.");

        if (string.IsNullOrWhiteSpace(studentName))
            throw new DomainValidationException("StudentName is required.");

        if (string.IsNullOrWhiteSpace(studentEmail))
            throw new DomainValidationException("StudentEmail is required.");

        if (string.IsNullOrWhiteSpace(collegeName))
            throw new DomainValidationException("CollegeName is required.");

        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainValidationException("CompanyName is required.");

        if (string.IsNullOrWhiteSpace(jobRole))
            throw new DomainValidationException("JobRole is required.");

        var application = new StudentApplication
        {
            Id = Guid.NewGuid(),
            DriveId = driveId,
            CollegeId = collegeId,
            StudentId = studentId,
            StudentName = studentName,
            StudentEmail = studentEmail,
            CollegeName = collegeName,
            CompanyName = companyName,
            JobRole = jobRole,
            Status = ApplicationStatus.Applied,
            AppliedOn = DateTime.UtcNow
        };

        application.RaiseDomainEvent(new Domain.Events.ApplicationCreatedDomainEvent(
            application.Id,
            application.DriveId,
            application.CollegeId,
            application.StudentId,
            application.StudentEmail
        ));

        return application;
    }

    public void UpdateStatus(ApplicationStatus newStatus)
    {
        if (Status == ApplicationStatus.Rejected)
            throw new ForbiddenException("Cannot change status of a rejected application.");

        if (Status == ApplicationStatus.Accepted && newStatus != ApplicationStatus.Withdrawn)
            throw new ForbiddenException("Can only change status of an accepted application to withdrawn.");

        if (Status == ApplicationStatus.Withdrawn)
            throw new ForbiddenException("Cannot change status of a withdrawn application.");

        var oldStatus = Status;
        Status = newStatus;
        SetUpdatedAt();

        RaiseDomainEvent(new Domain.Events.ApplicationStatusUpdatedDomainEvent(
            Id,
            DriveId,
            CollegeId,
            StudentId,
            StudentEmail,
            oldStatus.ToString(),
            newStatus.ToString()
        ));
    }

    public void Withdraw()
    {
        if (Status == ApplicationStatus.Rejected)
            throw new ForbiddenException("Cannot withdraw a rejected application.");

        if (Status == ApplicationStatus.Accepted)
            throw new ForbiddenException("Cannot withdraw an accepted application. Please contact the recruiter.");

        if (Status == ApplicationStatus.Withdrawn)
            throw new ForbiddenException("Application is already withdrawn.");

        Status = ApplicationStatus.Withdrawn;
        SetUpdatedAt();

        RaiseDomainEvent(new Domain.Events.ApplicationWithdrawnDomainEvent(
            Id,
            DriveId,
            CollegeId,
            StudentId,
            StudentEmail
        ));
    }
}