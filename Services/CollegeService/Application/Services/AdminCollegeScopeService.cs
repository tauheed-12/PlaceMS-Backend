using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;

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

    public async Task AssignCollegeToAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        if (collegeIds.Contains(collegeId)) return; // Already assigned, no action needed

        var newScope = AdminCollegeScope.Create(adminUserId, collegeId);
        await _repository.AddScopeAsync(newScope, ct);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task RemoveCollegeFromAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var existingScope = await _repository.GetScopeAsync(adminUserId, collegeId, ct);
        if (existingScope == null) return; // Scope not found, no action needed

        _repository.RemoveScope(existingScope);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task<List<CollegeShortDto>> GetCollegesDetailsByAdminIdAsync(Guid adminUserId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        var collegeDetails = await _collegeRepository.GetByIdsAsync(collegeIds, ct);

        List<CollegeShortDto> collegesWithTpoInfo = new List<CollegeShortDto>();

        foreach (var college in collegeDetails)
        {
            var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(college.Id, ct);
            if (tpo == null)
            {
                collegesWithTpoInfo.Add(new CollegeShortDto
                {
                    Id = college.Id,
                    Name = college.Name,
                    Code = college.Code,
                    City = college.City,
                    State = college.State,
                    VerificationStatus = college.VerificationStatus,
                    HasTpoAssigned = false
                });
            }
            else
            {
                collegesWithTpoInfo.Add(new CollegeShortDto
                {
                    Id = college.Id,
                    Name = college.Name,
                    Code = college.Code,
                    City = college.City,
                    State = college.State,
                    VerificationStatus = college.VerificationStatus,
                    HasTpoAssigned = true
                });
            }
        }
        return collegesWithTpoInfo;
    }

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
        foreach (var tpoDetail in tpoDetailsBatch)
        {
            var tpoDetails = tpoDetailsList.FirstOrDefault(t => t.UserId == tpoDetail.UserId);
            if (tpoDetails != null)
            {
                tpoDetails.FullName = tpoDetail.FullName;
                tpoDetails.Email = tpoDetail.Email;
                tpoDetails.PhoneNumber = tpoDetail.PhoneNumber;
                tpoDetails.VerificationStatus = tpoDetail.VerificationStatus.ToString();
                tpoDetails.CollegeCode = tpoDetail.CollegeCode;
                tpoDetails.CollegeName = tpoDetail.CollegeName;
                tpoDetails.CreatedAt = tpoDetail.CreatedAt;
            }
        }

        return tpoDetailsList;
    }

    public async Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct)
    {
        var collegeIds = await _repository.GetCollegeIdsByAdminIdAsync(adminUserId, ct);
        return collegeIds.Contains(collegeId);
    }
}