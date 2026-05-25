using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.Interfaces.Repositories;
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

    public async Task<(IEnumerable<College> Items, int TotalCount)> GetFilteredAsync(CollegeFilterRequestDto filter, CancellationToken ct = default)
    {
        var query = _context.Colleges.AsNoTracking(); // soft-delete filter already applied globally

        // --- filters ---
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Code.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.State))
            query = query.Where(c => c.State.ToLower() == filter.State.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(c => c.City.ToLower() == filter.City.Trim().ToLower());

        if (filter.AccountStatus.HasValue)
            query = query.Where(c => c.AccountStatus == filter.AccountStatus.Value);

        if (filter.HasTpoAssigned.HasValue)
        {
            var collegeIdsWithTpo = _context.CollegeTpos
                .Where(t => !t.IsDeleted)
                .Select(t => t.CollegeId);

            query = filter.HasTpoAssigned.Value
                ? query.Where(c => collegeIdsWithTpo.Contains(c.Id))
                : query.Where(c => !collegeIdsWithTpo.Contains(c.Id));
        }

        // --- count before pagination ---
        var totalCount = await query.CountAsync(ct);

        // --- pagination ---
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public IQueryable<College> GetQueryable() => _context.Colleges.AsQueryable();

    public void Update(College college) => _context.Colleges.Update(college);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
