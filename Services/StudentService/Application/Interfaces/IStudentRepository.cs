using SharedKernel.Models;
using StudentService.Domain.Entities;

namespace StudentService.Application.Interfaces;

public interface IStudentRepository
{
    Task<StudentProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<StudentProfile?> GetByUserIdWithAllAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<StudentProfile>> GetByCollegeIdAsync(Guid collegeId, int page, int pageSize, string? search, CancellationToken ct = default);
    Task AddAsync(StudentProfile profile, CancellationToken ct = default);
    void Update(StudentProfile profile);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}