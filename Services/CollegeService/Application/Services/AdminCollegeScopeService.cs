using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.Interfaces.Repositories;
using CollegeService.Domain.Entities;
using CollegeService.Application.Interfaces.Clients;
using SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using CollegeService.Application.DTOs.Requests;

namespace CollegeService.Application.Services;

public class AdminCollegeScopeService : IAdminCollegeScopeService
{
    private readonly IAdminCollegeScopeRepository _repository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly ICollegeTpoRepository _tpoRepository;
    private readonly IIdentityServiceClient _identityClient;

    public AdminCollegeScopeService(IAdminCollegeScopeRepository repository, ICollegeRepository collegeRepository, ICollegeTpoRepository tpoRepository, IIdentityServiceClient identityClient)
    {
        _repository = repository;
        _collegeRepository = collegeRepository;
        _tpoRepository = tpoRepository;
        _identityClient = identityClient;
    }

    public async Task<AdminCollegeScopeResponseDto> AssignCollegeToAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        if (collegeIds.Contains(collegeId)) return new AdminCollegeScopeResponseDto(); // Already assigned, no action needed

        var college = await _collegeRepository.GetByIdAsync(collegeId, ct)
            ?? throw new NotFoundException("College not found.");

        var newScope = AdminCollegeScope.Create(adminUserId, collegeId);
        await _repository.AddScopeAsync(newScope, ct);
        await _repository.SaveChangesAsync(ct);
        return new AdminCollegeScopeResponseDto
        {
            AdminId = adminUserId,
            CollegeId = collegeId,
            CollegeName = college.Name,
            CollegeCode = college.Code
        };
    }

    public async Task RemoveCollegeFromAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var existingScope = await _repository.GetScopeAsync(adminUserId, collegeId, ct);
        if (existingScope == null) return; // Scope not found, no action needed

        _repository.RemoveScope(existingScope);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task<List<Guid>> GetCollegesIdsByAdminIdAsync(Guid adminUserId, CancellationToken ct)
    {
        return await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
    }

    public async Task<PaginatedResponseDto<CollegeShortDto>> GetCollegesByAdminIdAsync(Guid adminId, CollegeFilterRequestDto filter, CancellationToken ct)
    {
        if (filter.PageNumber <= 0) filter.PageNumber = 1;

        if (filter.PageSize <= 0) filter.PageSize = 10;

        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminId, ct);

        var query = _collegeRepository.GetQueryable().Where(c => collegeIds.Contains(c.Id));

        // Search filter
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();

            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search));
        }

        // State filter
        if (!string.IsNullOrWhiteSpace(filter.State))
        {
            query = query.Where(c =>
                c.State == filter.State);
        }

        // Status filter
        if (filter.AccountStatus.HasValue)
        {
            query = query.Where(c =>
                c.AccountStatus == filter.AccountStatus.Value);
        }

        var totalCount = await query.CountAsync(ct);

        // Paginated data
        var colleges = await query
            .OrderBy(c => c.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
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
            AccountStatus = college.AccountStatus,
            HasTpoAssigned = tpoCollegeIds.Contains(college.Id)
        }).ToList();

        return new PaginatedResponseDto<CollegeShortDto>
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            Items = result
        };
    }

    public async Task<PaginatedResponseDto<TpoDetailsDto>> GetTposByAdminIdAsync(Guid adminId, TpoFilterRequestDto filter, CancellationToken ct)
    {
        if (filter.PageNumber <= 0)
            filter.PageNumber = 1;

        if (filter.PageSize <= 0)
            filter.PageSize = 10;

        var collegeIds = await _repository
            .GetCollegeIdsByAdminIdAsync(adminId, ct);

        var query = _tpoRepository
            .GetQueryable()
            .Where(t => collegeIds.Contains(t.CollegeId));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();

            query = query.Where(t =>
                t.FullName.ToLower().Contains(search) ||
                t.CollegeName.ToLower().Contains(search));
        }

        if (filter.IsPrimary.HasValue)
        {
            query = query.Where(t =>
                t.IsPrimary == filter.IsPrimary.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var tpos = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var collegeIdsFromTpos = tpos
            .Select(t => t.CollegeId)
            .Distinct()
            .ToList();

        var colleges = await _collegeRepository
            .GetByIdsAsync(collegeIdsFromTpos, ct);

        var collegeMap = colleges
            .ToDictionary(c => c.Id);

        var userDetails = await _identityClient
            .GetTpoDetailsByIdsBatchAsync(
                tpos.Select(t => t.TpoId),
                ct)
            ?? new List<TpoDetails>();

        var userMap = userDetails
            .ToDictionary(x => x.UserId);

        var result = tpos
            .Where(t =>
                userMap.ContainsKey(t.TpoId) &&
                collegeMap.ContainsKey(t.CollegeId))
            .Select(t =>
            {
                var user = userMap[t.TpoId];

                var college = collegeMap[t.CollegeId];

                return new TpoDetailsDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    CollegeId = college.Id,
                    CollegeName = college.Name,
                    CollegeCode = college.Code,

                    IsPrimary = t.IsPrimary,
                    IsActive = t.IsActive
                };
            })
            .ToList();

        return new PaginatedResponseDto<TpoDetailsDto>
        {
            Items = result,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)filter.PageSize)
        };
    }

    public async Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        return collegeIds.Contains(collegeId);
    }
}