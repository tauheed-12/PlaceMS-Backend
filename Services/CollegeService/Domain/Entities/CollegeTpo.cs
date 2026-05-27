using System.Runtime.CompilerServices;
using CollegeService.Domain.Events;
using SharedKernel.Abstractions;

namespace CollegeService.Domain.Entities;

public class CollegeTpo : AggregateRoot
{
    public Guid CollegeId { get; private set; }
    public Guid TpoId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string CollegeName { get; private set; } = string.Empty;
    public string CollegeCode { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? RemovedAt { get; private set; }
    public Guid AssignedById { get; private set; }
    public Guid? UpdatedById { get; private set; }

    private CollegeTpo() { }

    public static CollegeTpo Create(Guid collegeId, Guid tpoId, string fullName, string collegeName, string collegeCode, string email, Guid assignedBy, bool isPrimary = true, bool isActive = true)
    {
        var collegeTpo = new CollegeTpo
        {
            CollegeId = collegeId,
            FullName = fullName.Trim(),
            CollegeName = collegeName.Trim(),
            CollegeCode = collegeCode.Trim().ToUpperInvariant(),
            TpoId = tpoId,
            Email = email.Trim().ToLowerInvariant(),
            IsPrimary = isPrimary,
            IsActive = isActive,
            AssignedById = assignedBy
        };

        collegeTpo.SetUpdatedAt();

        collegeTpo.RaiseDomainEvent(new TpoAssignedToCollegeDomainEvent(
            collegeTpo.TpoId,
            collegeTpo.Email,
            collegeTpo.FullName,
            collegeTpo.CollegeId,
            collegeTpo.CollegeName,
            collegeTpo.CollegeCode,
            collegeTpo.AssignedById));

        return collegeTpo;
    }

    public void UpdateDetails(string? fullName = null, string? email = null, string? collegeName = null, string? collegeCode = null, Guid? updatedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            FullName = fullName.Trim();

        if (!string.IsNullOrWhiteSpace(email))
            Email = email.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(collegeName))
            CollegeName = collegeName.Trim();

        if (!string.IsNullOrWhiteSpace(collegeCode))
            CollegeCode = collegeCode.Trim().ToUpperInvariant();

        UpdatedById = updatedBy;

        SetUpdatedAt();
    }

    // ------------------ Status Management --------------------------------
    public void Deactivate(Guid deactivatedBy)
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedById = deactivatedBy;
        SetUpdatedAt();

        RaiseDomainEvent(new TpoDeactivatedDomainEvent(Id, CollegeId, deactivatedBy));
    }

    public void Activate(Guid activatedBy)
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedById = activatedBy;
        SetUpdatedAt();

        RaiseDomainEvent(new TpoActivatedDomainEvent(Id, CollegeId, activatedBy));
    }

    public void AssignPrimaryScope(Guid updatedBy)
    {
        if (!IsPrimary) return;

        IsPrimary = false;
        UpdatedById = updatedBy;
        SetUpdatedAt();

        RaiseDomainEvent(new TpoPrimaryScopeRemovedDomainEvent(Id, CollegeId, updatedBy));
    }

    public void RemovePrimaryScope(Guid updatedBy)
    {
        if (IsPrimary) return;

        IsPrimary = true;
        UpdatedById = updatedBy;
        SetUpdatedAt();

        RaiseDomainEvent(new TpoPrimaryScopeAddedDomainEvent(Id, CollegeId, updatedBy));
    }
}