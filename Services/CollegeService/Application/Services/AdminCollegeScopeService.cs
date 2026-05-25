using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.Interfaces.Repositories;
using CollegeService.Domain.Entities;
using CollegeService.Application.Interfaces.Clients;
using SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PaginatedResponseDto<CollegeShortDto>> GetCollegesByAdminIdAsync(Guid adminId, int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber <= 0) pageNumber = 1;

        if (pageSize <= 0) pageSize = 10;

        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminId, ct);

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
            AccountStatus = college.AccountStatus,
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

    public async Task<PaginatedResponseDto<TpoDetailsDto>> GetTposByAdminIdAsync(Guid adminId, int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber <= 0) pageNumber = 1;

        if (pageSize <= 0) pageSize = 10;

        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminId, ct);

        var query = _collegeRepository.GetQueryable().Where(c => collegeIds.Contains(c.Id));
        var totalCount = await query.CountAsync(ct);

        var colleges = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var pagedCollegeIds = colleges
        .Select(c => c.Id)
        .ToList();

        var tpos = await _tpoRepository.GetPrimaryTposByCollegeIdsAsync(pagedCollegeIds, ct);

        var userDetails = await _identityClient.GetTpoDetailsByIdsBatchAsync(tpos.Select(t => t.TpoId), ct)
            ?? new List<TpoDetails>();

        var userMap = userDetails.ToDictionary(x => x.UserId);

        var result = tpos
        .Where(t => userMap.ContainsKey(t.TpoId))
        .Select(t =>
        {
            var user = userMap[t.TpoId];

            var college = colleges
                .First(c => c.Id == t.CollegeId);

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
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize)
        };

    }

    public async Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        return collegeIds.Contains(collegeId);
    }
}