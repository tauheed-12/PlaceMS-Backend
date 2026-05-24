using SharedKernel.Abstractions;

namespace CollegeService.Domain.Entities;

public class AdminCollegeScope : AggregateRoot
{
    public Guid CollegeId { get; private set; }
    public Guid AdminUserId { get; private set; }

    public AdminCollegeScope() { }

    public static AdminCollegeScope Create(Guid collegeId, Guid adminUserId)
    {
        var adminCollegeScope = new AdminCollegeScope
        {
            CollegeId = collegeId,
            AdminUserId = adminUserId
        };

        adminCollegeScope.SetUpdatedAt();
        return adminCollegeScope;
    }
}