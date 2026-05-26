using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.Interfaces.Repositories;
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

    public void Update(CollegeTpo collegeTpo)
        => _context.CollegeTpos.Update(collegeTpo);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public IQueryable<CollegeTpo> GetQueryable()
        => _context.CollegeTpos.AsQueryable();

    public async Task<CollegeTpo?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct = default)
        => await _context.CollegeTpos.FirstOrDefaultAsync(
            t => t.CollegeId == collegeId &&
                 t.IsPrimary &&
                 t.IsActive,
            ct);

    public async Task<CollegeTpo?> GetByTpoIdAsync(Guid tpoId, CancellationToken ct = default)
    {
        return await _context.CollegeTpos.FirstOrDefaultAsync(
            t => t.TpoId == tpoId &&
                 t.IsActive);
    }

    public async Task<List<CollegeTpo>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct = default)
        => await _context.CollegeTpos
            .Where(t => t.CollegeId == collegeId)
            .ToListAsync(ct);

    public async Task<CollegeTpo?> GetTpoByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.CollegeTpos.FirstOrDefaultAsync(
            t => t.Email == normalizedEmail,
            ct);
    }

    public async Task<List<Guid>> GetCollegeIdsHavingPrimaryTpoAsync(List<Guid> collegeIds, CancellationToken ct)
    {
        return await _context.CollegeTpos
            .Where(t =>
                collegeIds.Contains(t.CollegeId) &&
                t.IsPrimary &&
                t.IsActive)
            .Select(t => t.CollegeId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<List<CollegeTpo>> GetPrimaryTposByCollegeIdsAsync(List<Guid> collegeIds, CancellationToken ct)
    {
        return await _context.CollegeTpos
                .Where(t =>
                    collegeIds.Contains(t.CollegeId) &&
                    t.IsPrimary &&
                    t.IsActive)
                .ToListAsync(ct);
    }

    public async Task<(IEnumerable<CollegeTpo> Items, int TotalCount)> GetTposAsync(TpoFilterRequestDto filter, CancellationToken ct = default)
    {
        if (filter.PageNumber <= 0)
            filter.PageNumber = 1;

        if (filter.PageSize <= 0)
            filter.PageSize = 10;

        var query = _context.CollegeTpos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var normalizedSearch = filter.Search
                .Trim()
                .ToLowerInvariant();

            query = query.Where(t =>
                t.FullName.ToLower().Contains(normalizedSearch) ||
                t.Email.ToLower().Contains(normalizedSearch));
        }

        if (filter.IsPrimary.HasValue)
        {
            query = query.Where(t =>
                t.IsPrimary == filter.IsPrimary.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}