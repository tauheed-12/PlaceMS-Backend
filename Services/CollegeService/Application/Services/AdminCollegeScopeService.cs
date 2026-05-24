using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;
using SharedKernel.Exceptions;

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

    public async Task<List<Guid>> GetCollegesByAdminIdAsync(Guid adminUserId, CancellationToken ct)
        => await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);

    public async Task<List<TpoDetailsDto>> GetTposByAdminIdAsync(Guid adminUserId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        var tpoDetailsList = new List<TpoDetailsDto>();
        var tpoIds = new HashSet<Guid>();

        foreach (var collegeId in collegeIds)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId, ct);
            if (college == null) continue;

            var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct);
            if (tpo == null) continue;

            tpoIds.Add(tpo.TpoId);
        }

        // Fetch details for all TPOs
        var tpoDetailsBatch = await _identityClient.GetTpoDetailsByIdsBatchAsync(tpoIds.ToList(), ct)
            ?? new List<TpoDetails>();

        return tpoDetailsBatch.Select(tpoDetail => new TpoDetailsDto
        {
            UserId = tpoDetail.UserId,
            FullName = tpoDetail.FullName,
            Email = tpoDetail.Email,
            PhoneNumber = tpoDetail.PhoneNumber,
            CollegeId = tpoDetail.CollegeId,
            CollegeCode = tpoDetail.CollegeCode,
            CollegeName = tpoDetail.CollegeName,
            VerificationStatus = tpoDetail.VerificationStatus.ToString(),
            IsPrimary = true,
            IsActive = true,
            CreatedAt = tpoDetail.CreatedAt
        }).ToList();
    }

    public async Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        return collegeIds.Contains(collegeId);
    }
}