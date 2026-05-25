using CollegeService.Domain.Events;
using SharedKernel.Abstractions;

namespace CollegeService.Domain.Entities;

public class CollegeTpo : AggregateRoot
{
    public Guid CollegeId { get; private set; }
    public Guid TpoId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string CollegeName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }
    public bool RemovedAt { get; private set; }
    public Guid AssignedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public CollegeTpo() { }

    public static CollegeTpo Create(Guid collegeId, Guid tpoId, string fullName, string collegeName, string email, Guid assignedBy, bool isPrimary = true, bool isActive = true)
    {
        var collegeTpo = new CollegeTpo
        {
            CollegeId = collegeId,
            FullName = fullName,
            CollegeName = collegeName,
            TpoId = tpoId,
            Email = email.Trim().ToLowerInvariant(),
            IsPrimary = isPrimary,
            IsActive = isActive,
            RemovedAt = false,
            AssignedBy = assignedBy
        };

        collegeTpo.SetUpdatedAt();
        collegeTpo.RaiseDomainEvent(new TpoAssignedToCollegeDomainEvent(tpoId, collegeId, collegeName, assignedBy));
        return collegeTpo;
    }

    public void Deactivate(Guid deactivatedBy)
    {
        if (!IsActive) return;

        IsActive = false;
        IsPrimary = false;
        RemovedAt = true;
        UpdatedBy = deactivatedBy;
        SetUpdatedAt();
        RaiseDomainEvent(new TpoDeactivatedDomainEvent(Id, CollegeId, deactivatedBy));
    }

    public void Activate(Guid activatedBy)
    {
        if (IsActive) return;
        IsActive = true;
        IsPrimary = true;
        UpdatedBy = activatedBy;
        SetUpdatedAt();
        RaiseDomainEvent(new TpoActivatedDomainEvent(Id, CollegeId, activatedBy));
    }
}