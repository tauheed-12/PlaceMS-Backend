using SharedKernel.Enums;

namespace ApplicationService.Application.Interfaces;

public interface IDriveServiceClient
{
    Task<InternalDriveDetail?> GetInternalDriveDetailAsync(Guid driveId, CancellationToken ct = default);
    Task<DriveCollegeStatus?> GetDriveCollegeStatusAsync(Guid driveId, Guid collegeId, CancellationToken ct = default);
}

public record InternalDriveDetail
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

public record DriveCollegeStatus
{
    public Guid DriveId { get; init; }
    public Guid CollegeId { get; init; }
    public bool IsApproved { get; init; }
    public bool IsDeactivated { get; init; }
    public bool IsDeadlinePassed { get; init; }
    public bool CanApply { get; init; }
}
