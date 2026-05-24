using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;
using SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace CollegeService.Application.Services;

public class CollegeQueryService : ICollegeQueryService
{
    private readonly ICollegeRepository _collegeRepository;
    private readonly IAdminCollegeScopeService _adminCollegeScopeService;

    public CollegeQueryService(ICollegeRepository collegeRepository, IAdminCollegeScopeService adminCollegeScopeService)
    {
        _collegeRepository = collegeRepository;
        _adminCollegeScopeService = adminCollegeScopeService;
    }

    public async Task<List<CollegeShortDto>> GetAllCollegesAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber <= 0) pageNumber = 1;

        if (pageSize <= 0) pageSize = 10;

        var query = _collegeRepository.GetQueryable();

        var colleges = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return [.. colleges.Select(c => MapToShortDto(c))];
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
            .Select(c => new CollegeShortDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                City = c.City,
                State = c.State,
                VerificationStatus = c.VerificationStatus
            })
            .ToListAsync(ct);

        return new PaginatedResponseDto<CollegeShortDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = colleges
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