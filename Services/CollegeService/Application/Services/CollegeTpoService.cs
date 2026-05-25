using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.Interfaces.Repositories;
using CollegeService.Application.Interfaces.Clients;
using CollegeService.Application.Interfaces;
using CollegeService.Domain.Entities;
using SharedKernel.Enums;
using SharedKernel.Exceptions;

namespace CollegeService.Application.Services;

public class CollegeTpoService : ICollegeTpoService
{
    private readonly ICollegeTpoRepository _tpoRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly IIdentityServiceClient _identityClient;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly ILogger<CollegeTpoService> _logger;

    public CollegeTpoService(
        ICollegeTpoRepository tpoRepository,
        ICollegeRepository collegeRepository,
        IIdentityServiceClient identityClient,
        IDomainEventPublisher eventPublisher,
        ILogger<CollegeTpoService> logger)
    {
        _tpoRepository = tpoRepository;
        _collegeRepository = collegeRepository;
        _identityClient = identityClient;
        _eventPublisher = eventPublisher;

        _logger = logger;
    }

    public async Task<TpoDetailsDto> AssignPrimaryTpoAsync(
        CreateTpoRequestDto request,
        Guid assignedBy,
        CancellationToken ct)
    {
        var college = await _collegeRepository.GetByIdAsync(request.CollegeId, ct)
            ?? throw new NotFoundException("College", request.CollegeId);

        if (college.AccountStatus == AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("Cannot assign a TPO to a deactivated college. Reactivate the college first.");

        var existingTpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(request.CollegeId, ct);
        if (existingTpo is not null)
            throw new BusinessRuleException($"College '{college.Name}' already has a primary TPO assigned" + "Remove the existing TPO before assigning a new one.");

        var emailTaken = await _tpoRepository.GetTpoByEmailAsync(request.Email, ct);
        if (emailTaken is not null)
            throw new ConflictException("TPO", "email", request.Email);

        var identityResult = await _identityClient.RegisterTpoAsync(
            new RegisterTpoIdentityRequestDto
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                CollegeId = request.CollegeId,
                CollegeCode = college.Code
            }, ct);

        if (identityResult is null)
            throw new ServiceUnavailableException("IdentityService", "Failed to create TPO user account. Please try again.");

        var collegeTpo = CollegeTpo.Create(
            collegeId: request.CollegeId,
            collegeName: college.Name,
            fullName: request.FullName,
            tpoId: identityResult.UserId,
            email: request.Email,
            assignedBy: assignedBy);

        await _tpoRepository.AddAsync(collegeTpo, ct);

        await _tpoRepository.SaveChangesAsync(ct);

        var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(request.CollegeId, ct)
            ?? throw new NotFoundException("TPO", college.Name);

        var tpoDetails = await _identityClient.GetTpoDetails(tpo.TpoId, ct)
            ?? throw new NotFoundException("TPO not found");

        _logger.LogInformation("TPO {TpoUserId} ({TpoEmail}) assigned to college {CollegeId} by admin {AssignedBy}",
            identityResult.UserId, request.Email, request.CollegeId, assignedBy);

        var events = college.DomainEvents.ToList();
        await _eventPublisher.PublishAsync(events, ct);
        college.ClearDomainEvents();

        return MapToDetailsDto(tpoDetails, college, tpo.IsPrimary);
    }

    public async Task RemoveTpoAsync(Guid collegeId, Guid userId, CancellationToken ct)
    {
        Guid deactivatedBy = Guid.NewGuid(); // TODO: update this from jwt token
        var college = await _collegeRepository.GetByIdAsync(collegeId, ct)
            ?? throw new NotFoundException("College", collegeId);

        var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct)
            ?? throw new NotFoundException(
                $"No primary TPO is assigned to college '{college.Name}'.");

        if (tpo.TpoId != userId)
            throw new BusinessRuleException("The specified user is not the primary TPO of this college.");

        tpo.Deactivate(deactivatedBy);
        _tpoRepository.Update(tpo);

        await _tpoRepository.SaveChangesAsync(ct);

        var events = college.DomainEvents.ToList();
        await _eventPublisher.PublishAsync(events, ct);
        college.ClearDomainEvents();

        _logger.LogInformation("TPO {TpoUserId} removed from college {CollegeId}", userId, collegeId);
    }

    public async Task<TpoDetailsDto?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId, ct)
            ?? throw new NotFoundException("College", collegeId);

        var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct)
            ?? throw new NotFoundException("TPO", college.Name);

        var tpoDetails = await _identityClient.GetTpoDetails(tpo.TpoId, ct)
            ?? throw new NotFoundException("");

        if (tpo is null) return null;

        return MapToDetailsDto(tpoDetails, college, tpo.IsPrimary);
    }

    public async Task<bool> IsPrimaryTpoAsync(Guid collegeId, Guid userId, CancellationToken ct)
    {
        var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct);
        return tpo is not null && tpo.TpoId == userId;
    }


    // fetch all TPOs with pagination and optional filters
    public async Task<PaginatedResponseDto<TpoDetailsDto>> GetTposAsync(TpoFilterRequestDto filter, CancellationToken ct)
    {
        if (filter.PageNumber <= 0)
            filter.PageNumber = 1;

        if (filter.PageSize <= 0)
            filter.PageSize = 10;

        // Step 1: Fetch paginated TPO mappings
        var (tpos, totalCount) = await _tpoRepository.GetTposAsync(filter, ct);

        // Step 2: Fetch college details
        var collegeIds = tpos
            .Select(t => t.CollegeId)
            .Distinct()
            .ToList();

        var colleges = await _collegeRepository
            .GetByIdsAsync(collegeIds, ct);

        var collegeMap = colleges
            .ToDictionary(c => c.Id);

        // Step 3: Fetch user details from IdentityService
        var userDetails = await _identityClient
            .GetTpoDetailsByIdsBatchAsync(
                tpos.Select(t => t.TpoId),
                ct)
            ?? new List<TpoDetails>();

        var userMap = userDetails
            .ToDictionary(u => u.UserId);

        // Step 4: Merge response
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

        // Step 5: Return paginated response
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

    public async Task<TpoDetailsDto> ActivatePrimaryTpoAsync(Guid tpoId, CancellationToken ct)
    {
        var details = await _identityClient.ActivateTpoAccount(tpoId, ct)
            ?? throw new NotFoundException("TPO not found");

        var collegeTpoDetails = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(tpoId, ct);

        return new TpoDetailsDto
        {
            UserId = details.UserId,
            FullName = details.FullName,
            Email = details.Email,
            PhoneNumber = details.PhoneNumber,
            CollegeId = details.CollegeId,
            VerificationStatus = details.VerificationStatus,
            AccountStatus = details.AccountStatus,
            CollegeCode = details.CollegeCode,
            CollegeName = details.CollegeName,
            // IsPrimary = collegeTpoDetails?.IsPrimary,
        };
    }

    public async Task<TpoDetailsDto> DeactivatePrimaryTpoAsync(Guid tpoId, CancellationToken ct)
    {
        var details = await _identityClient.DeactivateTpoAccount(tpoId, ct)
            ?? throw new NotFoundException("TPO not found");

        var collegeTpoDetails = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(tpoId, ct);

        return new TpoDetailsDto
        {
            UserId = details.UserId,
            FullName = details.FullName,
            Email = details.Email,
            PhoneNumber = details.PhoneNumber,
            CollegeId = details.CollegeId,
            VerificationStatus = details.VerificationStatus,
            AccountStatus = details.AccountStatus,
            CollegeCode = details.CollegeCode,
            CollegeName = details.CollegeName,
            // IsPrimary = collegeTpoDetails?.IsPrimary,
        };
    }


    private static TpoDetailsDto MapToDetailsDto(TpoDetails tpo, College college, bool isPrimary)
        => new()
        {
            UserId = tpo.UserId,
            CollegeId = college.Id,
            CollegeName = college.Name,
            CollegeCode = college.Code,
            FullName = tpo.FullName,
            Email = tpo.Email,
            PhoneNumber = tpo.PhoneNumber,
            IsPrimary = isPrimary,
            VerificationStatus = tpo.VerificationStatus,
            AccountStatus = tpo.AccountStatus,
            CreatedAt = tpo.CreatedAt
        };
}