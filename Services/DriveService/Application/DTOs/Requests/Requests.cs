// Requests/DriveRequests.cs
using SharedKernel.Enums;

namespace DriveService.Application.DTOs.Requests;

/// <summary>
/// Recruiter creates drive + targets colleges in one step.
/// </summary>
public record CreateDriveRequest
{
    public string CompanyName { get; init; } = string.Empty;
    public string JobRole { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public EmploymentType EmploymentType { get; init; }
    public DateTime DriveDate { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public double MinCgpa { get; init; }
    public EligibleBranch EligibleBranches { get; init; }
    public int EligibleBatch { get; init; }
    public List<string> Rounds { get; init; } = new();       // ["Online Assessment", "Technical Interview"]
    public List<Guid> TargetCollegeIds { get; init; } = new(); // Colleges to submit to
}

/// <summary>
/// Recruiter edits drive — only allowed if no college has approved yet.
/// </summary>
public record UpdateDriveRequest
{
    public string JobRole { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;
    public string CTC { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public EmploymentType EmploymentType { get; init; }
    public DateTime DriveDate { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public double MinCgpa { get; init; }
    public EligibleBranch EligibleBranches { get; init; }
    public int EligibleBatch { get; init; }
    public List<string> Rounds { get; init; } = new();
}

/// <summary>
/// TPO approves drive for their college.
/// Note is optional — shown to recruiter as confirmation message.
/// </summary>
public record ApproveDriveRequest
{
    public string? Note { get; init; }
}

/// <summary>
/// TPO rejects drive for their college.
/// Note is required — recruiter must know why.
/// </summary>
public record RejectDriveRequest
{
    public string Note { get; init; } = string.Empty;
}

/// <summary>
/// TPO requests changes — drive is locked until recruiter edits and resubmits.
/// Note is required — must describe what needs to change.
/// </summary>
public record RequestChangesRequest
{
    public string Note { get; init; } = string.Empty;
}

/// <summary>
/// Paginated query params for recruiter's drive list.
/// </summary>
public record DriveListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }              // Company name or role
    public bool? IsDeactivated { get; init; }         // Filter by active/deactivated
}

/// <summary>
/// Student browse drives query — filters by college + deadline.
/// </summary>
public record AvailableDrivesQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public EmploymentType? EmploymentType { get; init; }
    public bool EligibleOnly { get; init; } = false;  // Filter drives student is eligible for
}

/// <summary>
/// Admin query — all drives across platform.
/// </summary>
public record AdminDriveListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public Guid? CollegeId { get; init; }
    public Guid? RecruiterUserId { get; init; }
    public bool? IsDeactivated { get; init; }
}