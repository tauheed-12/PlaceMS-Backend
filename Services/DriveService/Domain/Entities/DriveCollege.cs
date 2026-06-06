using SharedKernel.Abstractions;
using SharedKernel.Enums;

namespace DriveService.Domain.Entities;

public class DriveCollege : BaseEntity
{
    private DriveCollege() { }

    public Guid DriveId { get; private set; }
    public Guid CollegeId { get; private set; }
    public string CollegeName { get; private set; } = string.Empty;
    public Guid? TpoUserId { get; private set; }
    public string? TpoEmail { get; private set; }
    public string? TpoName { get; private set; }
    public DriveApprovalStatus ApprovalStatus { get; private set; }
    public string? TpoNote { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Drive Drive { get; private set; } = null!;

    public static DriveCollege Create(
        Guid driveId,
        Guid collegeId,
        string collegeName,
        Guid? tpoUserId,
        string? tpoEmail,
        string? tpoName)
    {
        return new DriveCollege
        {
            DriveId = driveId,
            CollegeId = collegeId,
            CollegeName = collegeName,
            TpoUserId = tpoUserId,
            TpoEmail = tpoEmail,
            TpoName = tpoName,
            ApprovalStatus = DriveApprovalStatus.Pending
        };
    }

    public void Approve(Guid tpoUserId, string? note)
    {
        TpoUserId = tpoUserId;
        ApprovalStatus = DriveApprovalStatus.Approved;
        TpoNote = note;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(Guid tpoUserId, string reason)
    {
        TpoUserId = tpoUserId;
        ApprovalStatus = DriveApprovalStatus.Rejected;
        TpoNote = reason;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RequestChanges(Guid tpoUserId, string note)
    {
        TpoUserId = tpoUserId;
        ApprovalStatus = DriveApprovalStatus.ChangesRequested;
        TpoNote = note;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetToPending()
    {
        ApprovalStatus = DriveApprovalStatus.Pending;
        TpoNote = null;
        ReviewedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
