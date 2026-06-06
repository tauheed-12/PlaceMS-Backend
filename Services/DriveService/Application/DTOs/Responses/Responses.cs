// Responses/DriveResponses.cs
using SharedKernel.Enums;

namespace DriveService.Application.DTOs.Responses;

/// <summary>
/// Full drive detail — used by Recruiter, TPO, Admin, Internal.
/// </summary>
public record DriveDetailResponse
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public DateTime DriveDate { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public double MinCgpa { get; init; }
    public string EligibleBranches { get; init; } = string.Empty;
    public int EligibleBatch { get; init; }
    public bool IsDeactivated { get; init; }
    public bool CanEdit { get; init; }                // True if no college approved yet
    public List<string> Rounds { get; init; } = new();
    public List<DriveCollegeResponse> Colleges { get; init; } = new();
    public Guid RecruiterUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Per-college approval status embedded in DriveDetailResponse.
/// </summary>
public record DriveCollegeResponse
{
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string ApprovalStatus { get; init; } = string.Empty;  // Pending / Approved / Rejected / ChangesRequested
    public string? TpoNote { get; init; }
    public DateTime? ReviewedAt { get; init; }
}

/// <summary>
/// Lightweight item for paginated lists — Recruiter and Admin views.
/// </summary>
public record DriveListItemResponse
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public DateTime ApplicationDeadline { get; init; }
    public double MinCgpa { get; init; }
    public bool IsDeactivated { get; init; }
    public int TotalCollegesTargeted { get; init; }
    public int ApprovedCollegesCount { get; init; }
    public int PendingCollegesCount { get; init; }
    public int RejectedCollegesCount { get; init; }
    public int ChangesRequestedCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Drive as seen by a Student — no approval status, no recruiter info.
/// Only approved, non-deactivated drives with deadline in future.
/// </summary>
public record StudentDriveResponse
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public DateTime DriveDate { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public double MinCgpa { get; init; }
    public string EligibleBranches { get; init; } = string.Empty;
    public int EligibleBatch { get; init; }
    public List<string> Rounds { get; init; } = new();
    public bool HasApplied { get; init; }             // Populated by Application Service if needed
    public int DaysUntilDeadline { get; init; }
}

/// <summary>
/// Drive as seen by TPO in their pending list.
/// Includes full detail + which college this approval belongs to.
/// </summary>
public record TpoDriveResponse
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public DateTime DriveDate { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public double MinCgpa { get; init; }
    public string EligibleBranches { get; init; } = string.Empty;
    public int EligibleBatch { get; init; }
    public List<string> Rounds { get; init; } = new();
    public string ApprovalStatus { get; init; } = string.Empty;
    public string? TpoNote { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public int EligibleStudentCount { get; init; }     // Students matching CGPA + branch
    public DateTime ReceivedAt { get; init; }
}

/// <summary>
/// Returned after TPO action (approve / reject / request changes).
/// </summary>
public record DriveApprovalActionResponse
{
    public Guid DriveId { get; init; }
    public Guid CollegeId { get; init; }
    public string NewStatus { get; init; } = string.Empty;
    public string? Note { get; init; }
    public DateTime ReviewedAt { get; init; }
}

/// <summary>
/// Returned after drive creation.
/// </summary>
public record CreateDriveResponse
{
    public Guid DriveId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public int CollegesNotified { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Internal response for Application Service — full drive + college approval status.
/// </summary>
public record InternalDriveDetailResponse
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public double MinCgpa { get; init; }
    public EligibleBranch EligibleBranches { get; init; }
    public int EligibleBatch { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public bool IsDeactivated { get; init; }
    public Guid RecruiterUserId { get; init; }
}

/// <summary>
/// Internal check — is this drive approved for a specific college?
/// </summary>
public record DriveCollegeStatusResponse
{
    public Guid DriveId { get; init; }
    public Guid CollegeId { get; init; }
    public bool IsApproved { get; init; }
    public bool IsDeactivated { get; init; }
    public bool IsDeadlinePassed { get; init; }
    public bool CanApply { get; init; }    // IsApproved && !IsDeactivated && !IsDeadlinePassed
}