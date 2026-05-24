using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;
using SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using SharedKernel.Models;
using CollegeService.Application.DTOs.Requests;

namespace CollegeService.Application.Services;

public class CollegeQueryService : ICollegeQueryService
{
    private readonly ICollegeRepository _collegeRepository;
    private readonly IAdminCollegeScopeService _adminCollegeScopeService;
    private readonly ICollegeTpoRepository _tpoRepository;

    public CollegeQueryService(ICollegeRepository collegeRepository, IAdminCollegeScopeService adminCollegeScopeService, ICollegeTpoRepository tpoRepository)
    {
        _collegeRepository = collegeRepository;
        _adminCollegeScopeService = adminCollegeScopeService;
        _tpoRepository = tpoRepository;
    }

    public async Task<PaginatedResponseDto<CollegeShortDto>> GetAllCollegesAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber <= 0) pageNumber = 1;

        if (pageSize <= 0) pageSize = 10;

        var query = _collegeRepository.GetQueryable();
        var totalCount = await query.CountAsync(ct);

        var colleges = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var pagedCollegeIds = colleges
        .Select(c => c.Id)
        .ToList();

        var tpoCollegeIds = await _tpoRepository.GetCollegeIdsHavingPrimaryTpoAsync(pagedCollegeIds, ct);

        var result = colleges.Select(college => new CollegeShortDto
        {
            Id = college.Id,
            Name = college.Name,
            Code = college.Code,
            City = college.City,
            State = college.State,
            VerificationStatus = college.VerificationStatus,
            HasTpoAssigned = tpoCollegeIds.Contains(college.Id)
        }).ToList();

        return new PaginatedResponseDto<CollegeShortDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = result
        };
    }

    public async Task<CollegeDetailsDto> GetCollegeByIdAsync(Guid id, CancellationToken ct)
    {
        var college = await _collegeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("College not found");

        return MapToDetailsDto(college);
    }

    public async Task<CollegeDetailsDto> GetCollegeByCodeAsync(string code, CancellationToken ct)
    {
        var college = await _collegeRepository.GetByCodeAsync(code, ct)
            ?? throw new NotFoundException("College not found");

        return MapToDetailsDto(college);
    }

    public async Task<List<CollegeShortDto>> GetCollegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var colleges = await _collegeRepository.GetByIdsAsync(ids, ct);
        return [.. colleges.Select(c => MapToShortDto(c))];
    }

    public async Task<List<CollegeShortDto>> GetCollegesByCodesAsync(IEnumerable<string> codes, CancellationToken ct)
    {
        var colleges = await _collegeRepository.GetByCodesAsync(codes, ct);
        return [.. colleges.Select(c => MapToShortDto(c))];
    }

    public async Task<PaginatedResponseDto<CollegeShortDto>> GetCollegesByAdminIdAsync(Guid adminId, int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber <= 0) pageNumber = 1;

        if (pageSize <= 0) pageSize = 10;

        var collegeIds = await _adminCollegeScopeService.GetCollegesByAdminIdAsync(adminId, ct);

        var query = _collegeRepository.GetQueryable().Where(c => collegeIds.Contains(c.Id));
        var totalCount = await query.CountAsync(ct);

        // Paginated data
        var colleges = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var pagedCollegeIds = colleges
        .Select(c => c.Id)
        .ToList();

        var tpoCollegeIds = await _tpoRepository.GetCollegeIdsHavingPrimaryTpoAsync(pagedCollegeIds, ct);

        var result = colleges.Select(college => new CollegeShortDto
        {
            Id = college.Id,
            Name = college.Name,
            Code = college.Code,
            City = college.City,
            State = college.State,
            VerificationStatus = college.VerificationStatus,
            HasTpoAssigned = tpoCollegeIds.Contains(college.Id)
        }).ToList();

        return new PaginatedResponseDto<CollegeShortDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = result
        };
    }

    public async Task<PagedResult<CollegeShortDto>> GetFilteredCollegesAsync(CollegeFilterRequestDto filter, CancellationToken ct = default)
    {
        var (items, totalCount) = await _collegeRepository.GetFilteredAsync(filter, ct);

        return new PagedResult<CollegeShortDto>
        {
            Items = items.Select(c => MapToShortDto(c)).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
        };

    }

    public async Task<ValidateCollegeCodeResponseDto> ValidateCollegeAsync(string code, CancellationToken ct)
    {
        var college = await _collegeRepository.GetByCodeAsync(code, ct);
        return new ValidateCollegeCodeResponseDto
        {
            IsValid = college != null,
            VerificationStatus = college == null ? VerificationStatus.Unverified : college.VerificationStatus,
            CollegeId = college?.Id ?? Guid.Empty,
            CollegeName = college?.Name ?? string.Empty,
            CollegeCode = college?.Code ?? string.Empty,
            IsActive = college?.VerificationStatus != VerificationStatus.Deactivated
        };
    }

    private static CollegeShortDto MapToShortDto(College college, bool hasTpoAssigned = false)
    {
        return new CollegeShortDto
        {
            Id = college.Id,
            Name = college.Name,
            Code = college.Code,
            City = college.City,
            State = college.State,
            VerificationStatus = college.VerificationStatus,
            HasTpoAssigned = hasTpoAssigned
        };
    }

    private static CollegeDetailsDto MapToDetailsDto(College college)
    {
        return new CollegeDetailsDto
        {
            Id = college.Id,
            Name = college.Name,
            Code = college.Code,
            Email = college.Email,
            City = college.City,
            State = college.State,
            AffiliatedBy = college.AffiliatedBy,
            Type = college.Type,
            Phone = college.Phone,
            Website = college.Website,
            Pincode = college.Pincode,
            VerificationStatus = college.VerificationStatus
        };
    }
}