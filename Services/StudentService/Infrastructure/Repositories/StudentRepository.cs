using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using StudentService.Application.Interfaces;
using StudentService.Domain.Entities;
using StudentService.Infrastructure.Persistence;

namespace StudentService.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;

    public StudentRepository(StudentDbContext context) => _context = context;

    public async Task<StudentProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<StudentProfile?> GetByUserIdWithAllAsync(Guid userId, CancellationToken ct = default)
        => await _context.StudentProfiles
            .Include(s => s.Educations)
            .Include(s => s.Skills)
            .Include(s => s.Projects)
            .Include(s => s.Certifications)
            .Include(s => s.ResumeFiles)
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.StudentProfiles.AnyAsync(s => s.UserId == userId, ct);

    public async Task<PagedResult<StudentProfile>> GetByCollegeIdAsync(
        Guid collegeId, int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = _context.StudentProfiles
            .Where(s => s.CollegeId == collegeId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s =>
                s.FullName.ToLower().Contains(search.ToLower()) ||
                s.Email.ToLower().Contains(search.ToLower()) ||
                s.Branch.ToLower().Contains(search.ToLower()));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<StudentProfile>.Create(items, total, page, pageSize);
    }

    public async Task AddAsync(StudentProfile profile, CancellationToken ct = default)
        => await _context.StudentProfiles.AddAsync(profile, ct);

    public void Update(StudentProfile profile)
        => _context.StudentProfiles.Update(profile);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}