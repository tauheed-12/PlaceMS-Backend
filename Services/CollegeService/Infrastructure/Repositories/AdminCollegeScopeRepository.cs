using CollegeService.Application.Interfaces.Repositories;
using CollegeService.Domain.Entities;
using CollegeService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CollegeService.Infrastructure.Repositories;

public class AdminCollegeScopeRepository : IAdminCollegeScopeRepository
{
    private readonly CollegeDbContext _context;

    public AdminCollegeScopeRepository(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task AddScopeAsync(AdminCollegeScope scope, CancellationToken ct = default)
        => await _context.AdminCollegeScopes.AddAsync(scope, ct);

    public async Task<AdminCollegeScope?> GetScopeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct = default)
        => await _context.AdminCollegeScopes
            .FirstOrDefaultAsync(s => s.AdminUserId == adminUserId && s.CollegeId == collegeId, ct);

    public async Task<List<Guid>> GetCollegeIdsByAdminIdAsync(Guid adminId, CancellationToken ct = default)
        => await _context.AdminCollegeScopes
            .Where(s => s.AdminUserId == adminId)
            .Select(s => s.CollegeId)
            .ToListAsync(ct);

    public IQueryable<AdminCollegeScope> GetQueryable()
        => _context.AdminCollegeScopes;

    public void RemoveScope(AdminCollegeScope scope)
        => _context.AdminCollegeScopes.Remove(scope);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
