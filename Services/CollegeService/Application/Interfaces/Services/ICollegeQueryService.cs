using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using SharedKernel.Models;

namespace CollegeService.Application.Interfaces.Services;

public interface ICollegeQueryService
{
    Task<PaginatedResponseDto<CollegeShortDto>> GetAllCollegesAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<CollegeDetailsDto> GetCollegeByIdAsync(Guid id, CancellationToken ct);
    Task<CollegeDetailsDto> GetCollegeByCodeAsync(string code, CancellationToken ct);
    Task<List<CollegeShortDto>> GetCollegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<List<CollegeShortDto>> GetCollegesByCodesAsync(IEnumerable<string> codes, CancellationToken ct);
    Task<PagedResult<CollegeShortDto>> GetFilteredCollegesAsync(CollegeFilterRequestDto filter, CancellationToken ct = default);

    // Task<PaginatedResponseDto<CollegeShortDto>> GetCollegesByAdminIdAsync(
    //     Guid adminId,
    //     int pageNumber,
    //     int pageSize,
    //     CancellationToken ct);

    Task<ValidateCollegeCodeResponseDto> ValidateCollegeAsync(string code, CancellationToken ct);
}