using SharedKernel.Abstractions;
using SharedKernel.Enums;

namespace DriveService.Domain.Entities;

public class Drive : AggregateRoot
{
    private readonly List<DriveCollege> _driveColleges = [];
    private readonly List<DriveRound> _driveRounds = [];

    private Drive() { }

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
    public DriveStatus DriveStatus { get; private set; }
    public bool IsDeactivated { get; private set; }
    public IReadOnlyCollection<DriveCollege> DriveColleges => _driveColleges;
    public IReadOnlyCollection<DriveRound> DriveRounds => _driveRounds;

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
        int eligibleBatch)
    {
        return new Drive
        {
            Id = Guid.NewGuid(),
            RecruiterUserId = recruiterUserId,
            CompanyName = companyName,
            JobRole = jobRole,
            JobDescription = jobDescription,
            CTC = ctc,
            Location = location,
            EmploymentType = employmentType,
            DriveDate = driveDate,
            ApplicationDeadline = applicationDeadline,
            MinCgpa = minCgpa,
            EligibleBranches = eligibleBranches,
            EligibleBatch = eligibleBatch,
            DriveStatus = DriveStatus.Active,
            IsDeactivated = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddCollege(
        Guid collegeId,
        string collegeName,
        Guid tpoUserId)
    {
        _driveColleges.Add(
            DriveCollege.Create(
                Id,
                collegeId,
                collegeName,
                tpoUserId));
    }

    public void AddRound(
        int roundNumber,
        string roundName)
    {
        _driveRounds.Add(
            DriveRound.Create(
                Id,
                roundNumber,
                roundName));
    }

    public void Deactivate()
    {
        IsDeactivated = true;
        DriveStatus = DriveStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsDeactivated = false;
        DriveStatus = DriveStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}