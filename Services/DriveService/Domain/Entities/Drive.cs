using SharedKernel.Abstractions;
using SharedKernel.Enums;
using SharedKernel.Exceptions;
using DriveService.Domain.Events;

namespace DriveService.Domain.Entities;

public class Drive : AggregateRoot
{
    // ── Core Info ─────────────────────────────────────────────────
    public Guid RecruiterUserId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string JobRole { get; private set; } = string.Empty;
    public string JobDescription { get; private set; } = string.Empty;
    public string CTC { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public EmploymentType EmploymentType { get; private set; }
    public DateTime DriveDate { get; private set; }
    public DateTime ApplicationDeadline { get; private set; }
    public double MinCgpa { get; private set; }
    public EligibleBranch EligibleBranches { get; private set; }
    public int EligibleBatch { get; private set; }
    public bool IsDeactivated { get; private set; } = false;
    public DateTime? DeactivatedAt { get; private set; }

    // ── Navigation ────────────────────────────────────────────────
    private readonly List<DriveCollege> _driveColleges = new();
    private readonly List<DriveRound> _driveRounds = new();

    public IReadOnlyCollection<DriveCollege> DriveColleges => _driveColleges.AsReadOnly();
    public IReadOnlyCollection<DriveRound> DriveRounds => _driveRounds.AsReadOnly();

    // EF Core constructor
    private Drive() { }


    public static Drive Create(
        Guid recruiterUserId,
        string companyName,
        string jobRole,
        string jobDescription,
        string ctc,
        string location,
        EmploymentType employmentType,
        DateTime driveDate,
        DateTime applicationDeadline,
        double minCgpa,
        EligibleBranch eligibleBranches,
        int eligibleBatch,
        List<string> rounds,
        List<(Guid CollegeId, string CollegeName, Guid? TpoUserId, string? TpoEmail, string? TpoName)> colleges)
    {
        if (applicationDeadline <= DateTime.UtcNow)
            throw new DomainValidationException("Application deadline must be in the future.");

        if (driveDate <= applicationDeadline)
            throw new DomainValidationException("Drive date must be after the application deadline.");

        if (minCgpa is < 0 or > 10)
            throw new DomainValidationException("Minimum CGPA must be between 0 and 10.");

        if (!colleges.Any())
            throw new DomainValidationException("At least one target college must be selected.");

        if (!rounds.Any())
            throw new DomainValidationException("At least one interview round must be specified.");

        var drive = new Drive
        {
            RecruiterUserId = recruiterUserId,
            CompanyName = companyName.Trim(),
            JobRole = jobRole.Trim(),
            JobDescription = jobDescription.Trim(),
            CTC = ctc.Trim(),
            Location = location.Trim(),
            EmploymentType = employmentType,
            DriveDate = driveDate.ToUniversalTime(),
            ApplicationDeadline = applicationDeadline.ToUniversalTime(),
            MinCgpa = minCgpa,
            EligibleBranches = eligibleBranches,
            EligibleBatch = eligibleBatch
        };

        for (int i = 0; i < rounds.Count; i++)
            drive._driveRounds.Add(DriveRound.Create(drive.Id, i + 1, rounds[i]));

        // Add per-college approval records
        foreach (var college in colleges)
        {
            var driveCollege = DriveCollege.Create(
                drive.Id,
                college.CollegeId,
                college.CollegeName,
                college.TpoUserId,
                college.TpoEmail,
                college.TpoName);

            drive._driveColleges.Add(driveCollege);
        }

        drive.RaiseDomainEvent(new DriveCreatedDomainEvent(
            drive.Id,
            drive.CompanyName,
            drive.JobRole,
            drive.RecruiterUserId,
            drive.ApplicationDeadline,
            drive._driveColleges.Select(dc => new DriveCollegeInfo(
                dc.CollegeId, dc.CollegeName, dc.TpoUserId, dc.TpoEmail, dc.TpoName)).ToList()));

        return drive;
    }

    public void Update(
        string jobRole,
        string jobDescription,
        string ctc,
        string location,
        EmploymentType employmentType,
        DateTime driveDate,
        DateTime applicationDeadline,
        double minCgpa,
        EligibleBranch eligibleBranches,
        int eligibleBatch,
        List<string> rounds)
    {
        if (!CanEdit())
            throw new InvalidOperationDomainException(
                "Drive cannot be edited after a college has approved it.");

        if (IsDeactivated)
            throw new InvalidOperationDomainException("Cannot edit a deactivated drive.");

        if (applicationDeadline <= DateTime.UtcNow)
            throw new DomainValidationException("Application deadline must be in the future.");

        if (driveDate <= applicationDeadline)
            throw new DomainValidationException("Drive date must be after the application deadline.");

        if (minCgpa is < 0 or > 10)
            throw new DomainValidationException("Minimum CGPA must be between 0 and 10.");

        JobRole = jobRole.Trim();
        JobDescription = jobDescription.Trim();
        CTC = ctc.Trim();
        Location = location.Trim();
        EmploymentType = employmentType;
        DriveDate = driveDate.ToUniversalTime();
        ApplicationDeadline = applicationDeadline.ToUniversalTime();
        MinCgpa = minCgpa;
        EligibleBranches = eligibleBranches;
        EligibleBatch = eligibleBatch;

        // Replace rounds
        _driveRounds.Clear();
        for (int i = 0; i < rounds.Count; i++)
            _driveRounds.Add(DriveRound.Create(Id, i + 1, rounds[i]));

        // Reset ChangesRequested colleges to Pending — they need to re-review
        foreach (var dc in _driveColleges.Where(dc =>
            dc.ApprovalStatus == DriveApprovalStatus.ChangesRequested))
        {
            dc.ResetToPending();
        }

        SetUpdatedAt();

        RaiseDomainEvent(new DriveUpdatedDomainEvent(Id, CompanyName, JobRole, RecruiterUserId));
    }

    // TPO Actions
    public void Approve(Guid collegeId, Guid tpoUserId, string? note)
    {
        var driveCollege = GetDriveCollegeOrThrow(collegeId);
        driveCollege.Approve(tpoUserId, note);
        SetUpdatedAt();

        RaiseDomainEvent(new DriveApprovedDomainEvent(
            Id, CompanyName, JobRole, collegeId,
            driveCollege.CollegeName, tpoUserId, note));
    }

    public void Reject(Guid collegeId, Guid tpoUserId, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new DomainValidationException("A rejection note is required.");

        var driveCollege = GetDriveCollegeOrThrow(collegeId);
        driveCollege.Reject(tpoUserId, note);
        SetUpdatedAt();

        RaiseDomainEvent(new DriveRejectedDomainEvent(
            Id, CompanyName, JobRole, collegeId,
            driveCollege.CollegeName, tpoUserId, note, RecruiterUserId));
    }

    public void RequestChanges(Guid collegeId, Guid tpoUserId, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new DomainValidationException("A note describing required changes is mandatory.");

        var driveCollege = GetDriveCollegeOrThrow(collegeId);
        driveCollege.RequestChanges(tpoUserId, note);
        SetUpdatedAt();

        RaiseDomainEvent(new DriveChangesRequestedDomainEvent(
            Id, CompanyName, JobRole, collegeId,
            driveCollege.CollegeName, tpoUserId, note, RecruiterUserId));
    }

    // ── Resubmit ─────────────────────────────────────────────────
    public void ResubmitToCollege(Guid collegeId, Guid recruiterUserId)
    {
        if (RecruiterUserId != recruiterUserId)
            throw new ForbiddenException("You can only resubmit your own drives.");

        var driveCollege = GetDriveCollegeOrThrow(collegeId);

        if (driveCollege.ApprovalStatus != DriveApprovalStatus.ChangesRequested)
            throw new InvalidOperationDomainException(
                "Resubmission is only allowed when the college has requested changes.");

        driveCollege.ResetToPending();
        SetUpdatedAt();

        RaiseDomainEvent(new DriveResubmittedDomainEvent(
            Id, CompanyName, JobRole, collegeId,
            driveCollege.CollegeName, driveCollege.TpoUserId, RecruiterUserId));
    }

    // ── Deactivate ────────────────────────────────────────────────

    public void Deactivate()
    {
        if (IsDeactivated)
            throw new InvalidOperationDomainException("Drive is already deactivated.");

        IsDeactivated = true;
        DeactivatedAt = DateTime.UtcNow;
        SetUpdatedAt();

        RaiseDomainEvent(new DriveDeactivatedDomainEvent(
            Id, CompanyName, JobRole, RecruiterUserId,
            _driveColleges.Select(dc => dc.CollegeId).ToList()));
    }

    // ── Business Rule Helpers ─────────────────────────────────────
    public bool CanEdit()
        => !_driveColleges.Any(dc => dc.ApprovalStatus == DriveApprovalStatus.Approved);


    public bool IsLive()
        => !IsDeactivated &&
           _driveColleges.Any(dc => dc.ApprovalStatus == DriveApprovalStatus.Approved);

    public bool IsApprovedForCollege(Guid collegeId)
        => _driveColleges.Any(dc =>
            dc.CollegeId == collegeId &&
            dc.ApprovalStatus == DriveApprovalStatus.Approved);

    public bool IsDeadlinePassed()
        => DateTime.UtcNow > ApplicationDeadline;

    // ── Private Helpers ───────────────────────────────────────────

    private DriveCollege GetDriveCollegeOrThrow(Guid collegeId)
        => _driveColleges.FirstOrDefault(dc => dc.CollegeId == collegeId)
           ?? throw new NotFoundException($"This drive does not target college '{collegeId}'.");
}