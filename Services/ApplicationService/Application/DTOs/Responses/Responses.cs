using SharedKernel.Enums;
namespace ApplicationService.Application.DTOs.Responses;

public record ApplicationShortDto
{
    public Guid ApplicationId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public DateTime AppliedOn { get; init; }
}


// Detailed info for a single application, used in Recruiter and TPO views.
public record ApplicationDetailsDto
{
    public Guid ApplicationId { get; init; }
    public Guid DriveId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public DateTime AppliedOn { get; init; }
}

public record CreateApplicationResponseDto
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime AppliedOn { get; init; }
    public string Message { get; init; }
        = "Application submitted successfully.";
}

public record UpdateApplicationStatusResponseDto
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime UpdatedOn { get; init; }
    public string Message { get; init; }
        = "Application status updated successfully.";
}

public record WithdrawApplicationResponseDto
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime UpdatedOn { get; init; }
    public string Message { get; init; }
        = "Application withdrawn successfully.";
}

// For Student's view of their own applications.
public record StudentApplicationDto
{
    public Guid ApplicationId { get; init; }
    public Guid DriveId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public DateTime AppliedOn { get; init; }
}

public record ApplicationStatisticsDto
{
    public int TotalApplications { get; init; }
    public int TotalApplied { get; init; }
    public int TotalUnderReview { get; init; }
    public int TotalShortlisted { get; init; }
    public int TotalOffered { get; init; }
    public int TotalAccepted { get; init; }
    public int TotalRejected { get; init; }
    public int TotalWithdrawn { get; init; }
}