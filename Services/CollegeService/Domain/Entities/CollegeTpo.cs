using SharedKernel.Abstractions;

namespace CollegeService.Domain.Entities;

public class CollegeTpo : AggregateRoot
{
    public Guid CollegeId { get; private set; }
    public Guid TpoId { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }
    public bool? RemovedAt { get; private set; }
    public Guid AssignedBy { get; private set; }
    public CollegeTpo() { }

    public static CollegeTpo Create(Guid collegeId, Guid tpoId, Guid assignedBy, bool isPrimary = true, bool isActive = true)
    {
        var collegeTpo = new CollegeTpo
        {
            CollegeId = collegeId,
            TpoId = tpoId,
            IsPrimary = isPrimary,
            IsActive = isActive,
            AssignedBy = assignedBy
        };

        collegeTpo.SetUpdatedAt();
        return collegeTpo;
    }
}