using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;
using CollegeService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CollegeService.Infrastructure.Repositories;

public class CollegeTpoRepository : ICollegeTpoRepository
{
    private readonly CollegeDbContext _context;

    public CollegeTpoRepository(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CollegeTpo collegeTpo, CancellationToken ct = default)
        => await _context.CollegeTpos.AddAsync(collegeTpo, ct);

    public async Task<CollegeTpo?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct = default)
        => await _context.CollegeTpos
            .FirstOrDefaultAsync(t => t.CollegeId == collegeId && t.IsPrimary && t.IsActive, ct);

    public async Task<CollegeTpo?> GetTpoByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.CollegeTpos
            .FirstOrDefaultAsync(t => t.Email == normalizedEmail, ct);
    }

    public async Task<List<CollegeTpo>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct = default)
        => await _context.CollegeTpos
            .Where(t => t.CollegeId == collegeId)
            .ToListAsync(ct);

    public async Task<List<Guid>> GetCollegeIdsHavingPrimaryTpoAsync(List<Guid> collegeIds, CancellationToken ct)
        => await _context.CollegeTpos
            .Where(t => collegeIds.Contains(t.CollegeId) && t.IsPrimary && t.IsActive)
            .Select(t => t.CollegeId)
            .ToListAsync(ct);

    public void Update(CollegeTpo collegeTpo)
        => _context.CollegeTpos.Update(collegeTpo);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
