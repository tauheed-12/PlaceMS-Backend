using CollegeService.Domain.Entities;
using CollegeService.Application.DTOs.Requests;

namespace CollegeService.Application.Interfaces.Repositories;

public interface ICollegeTpoRepository
{
    Task<CollegeTpo?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct = default);
    Task<CollegeTpo?> GetTpoByEmailAsync(string email, CancellationToken ct = default);
    Task<List<CollegeTpo>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct = default);
    Task<List<Guid>> GetCollegeIdsHavingPrimaryTpoAsync(List<Guid> collegeIds, CancellationToken ct);
    Task<List<CollegeTpo>> GetPrimaryTposByCollegeIdsAsync(List<Guid> pagedCollegeIds, CancellationToken ct);
    // Task<(IEnumerable<College> Items, int TotalCount)> GetFilteredAsync(CollegeFilterRequestDto filter, CancellationToken ct = default);
    Task AddAsync(CollegeTpo collegeTpo, CancellationToken ct = default);
    void Update(CollegeTpo collegeTpo);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}