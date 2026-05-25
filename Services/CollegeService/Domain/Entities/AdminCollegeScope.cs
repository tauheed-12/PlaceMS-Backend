using CollegeService.Domain.Events;
using SharedKernel.Abstractions;

namespace CollegeService.Domain.Entities;

public class AdminCollegeScope : AggregateRoot
{
    public Guid CollegeId { get; private set; }
    public Guid AdminUserId { get; private set; }
    public string CollegeName { get; private set; } = string.Empty;
    public string CollegeCode { get; private set; } = string.Empty;
    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public AdminCollegeScope() { }

    public static AdminCollegeScope Create(Guid collegeId, string collegeName, string collegeCode, Guid adminUserId, Guid createdBy)
    {
        var adminCollegeScope = new AdminCollegeScope
        {
            CollegeId = collegeId,
            AdminUserId = adminUserId,
            CollegeName = collegeName,
            CollegeCode = collegeCode,
            CreatedBy = createdBy
        };

        adminCollegeScope.SetUpdatedAt();
        adminCollegeScope.RaiseDomainEvent(new CollegeAssignedToAdminDomainEvent(adminUserId, collegeId, collegeName, collegeCode, createdBy));
        return adminCollegeScope;
    }

    public void RemoveScope(Guid RemovedBy)
    {
        RaiseDomainEvent(new CollegeUnassignedFromAdminDomainEvent(AdminUserId, CollegeId, CollegeName, CollegeCode, RemovedBy));
    }
}