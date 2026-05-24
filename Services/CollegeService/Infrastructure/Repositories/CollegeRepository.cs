using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;
using CollegeService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CollegeService.Infrastructure.Repositories;

public class CollegeRepository : ICollegeRepository
{
    private readonly CollegeDbContext _context;

    public CollegeRepository(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(College college, CancellationToken ct = default)
        => await _context.Colleges.AddAsync(college, ct);

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => await _context.Colleges
            .AnyAsync(c => c.Code == code.Trim().ToUpperInvariant(), ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _context.Colleges
            .AnyAsync(c => c.Email == email.Trim().ToLowerInvariant(), ct);

    public async Task<College?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _context.Colleges
            .FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpperInvariant(), ct);

    public async Task<College?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Colleges
            .FirstOrDefaultAsync(c => c.Email == email.Trim().ToLowerInvariant(), ct);

    public async Task<College?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Colleges.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<College>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (!idList.Any()) return new List<College>();

        return await _context.Colleges
            .Where(c => idList.Contains(c.Id))
            .ToListAsync(ct);
    }

    public async Task<List<College>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        var normalizedCodes = codes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (!normalizedCodes.Any()) return new List<College>();

        return await _context.Colleges
            .Where(c => normalizedCodes.Contains(c.Code))
            .ToListAsync(ct);
    }

    public IQueryable<College> GetQueryable()
        => _context.Colleges.AsQueryable();

    public void Update(College college)
        => _context.Colleges.Update(college);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
