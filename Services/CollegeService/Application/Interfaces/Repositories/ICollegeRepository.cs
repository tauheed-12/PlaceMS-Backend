using CollegeService.Domain.Entities;
using CollegeService.Application.DTOs.Requests;

public interface ICollegeRepository
{
    Task<College?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<College?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<College?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<List<College>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<List<College>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);

    Task<(IEnumerable<College> Items, int TotalCount)> GetFilteredAsync(CollegeFilterRequestDto filter, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task AddAsync(College college, CancellationToken ct = default);
    void Update(College college);
    public IQueryable<College> GetQueryable();

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}