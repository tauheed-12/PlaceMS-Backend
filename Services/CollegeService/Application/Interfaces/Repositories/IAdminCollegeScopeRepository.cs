using CollegeService.Domain.Entities;

namespace CollegeService.Application.Interfaces.Repositories;

public interface IAdminCollegeScopeRepository
{
    Task<List<Guid>> GetCollegeIdsByAdminIdAsync(Guid adminId, CancellationToken ct = default);
    Task AddScopeAsync(AdminCollegeScope scope, CancellationToken ct = default);
    Task<AdminCollegeScope?> GetScopeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct = default);
    void RemoveScope(AdminCollegeScope scope);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}