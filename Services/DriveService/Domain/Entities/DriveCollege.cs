using SharedKernel.Abstractions;
using DriveService.Domain.Enums;

namespace DriveService.Domain.Entities;

public class DriveCollege : BaseEntity
{
    private DriveCollege() { }
    public Guid Id { get; private set; }
    public Guid DriveId { get; private set; }
    public Guid CollegeId { get; private set; }
    public string CollegeName { get; private set; } = string.Empty;
    public Guid TpoUserId { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public string? TpoNote { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Drive Drive { get; private set; } = null!;

    public static DriveCollege Create(
        Guid driveId,
        Guid collegeId,
        string collegeName,
        Guid tpoUserId)
    {
        return new DriveCollege
        {
            Id = Guid.NewGuid(),
            DriveId = driveId,
            CollegeId = collegeId,
            CollegeName = collegeName,
            TpoUserId = tpoUserId,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        ApprovalStatus = ApprovalStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        ApprovalStatus = ApprovalStatus.Rejected;
        TpoNote = reason;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RequestChanges(string note)
    {
        ApprovalStatus = ApprovalStatus.ChangesRequested;
        TpoNote = note;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}