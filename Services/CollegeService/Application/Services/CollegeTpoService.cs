using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
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
        Guid collegeId,
        CreateTpoRequestDto request,
        Guid assignedBy,
        CancellationToken ct)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId, ct)
            ?? throw new NotFoundException("College", collegeId);

        if (college.VerificationStatus == VerificationStatus.Deactivated)
            throw new InvalidOperationDomainException("Cannot assign a TPO to a deactivated college. Reactivate the college first.");

        var existingTpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct);
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
                CollegeId = collegeId,
                CollegeCode = college.Code
            }, ct);

        if (identityResult is null)
            throw new ServiceUnavailableException("IdentityService", "Failed to create TPO user account. Please try again.");

        var collegeTpo = CollegeTpo.Create(
            collegeId: collegeId,
            tpoId: identityResult.UserId,
            assignedBy: assignedBy);

        await _tpoRepository.AddAsync(collegeTpo, ct);

        await _tpoRepository.SaveChangesAsync(ct);

        // var events = college.DomainEvents.ToList();
        // await _eventPublisher.PublishAsync(events, ct);
        // college.ClearDomainEvents();
        var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct)
            ?? throw new NotFoundException("TPO", college.Name);

        var tpoDetails = await _identityClient.GetTpoDetails(tpo.TpoId, ct)
            ?? throw new NotFoundException("");

        _logger.LogInformation("TPO {TpoUserId} ({TpoEmail}) assigned to college {CollegeId} by admin {AssignedBy}",
            identityResult.UserId, request.Email, collegeId, assignedBy);

        return MapToDetailsDto(tpoDetails, college, tpo.IsPrimary);
    }

    public async Task RemoveTpoAsync(Guid collegeId, Guid userId, CancellationToken ct)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId, ct)
            ?? throw new NotFoundException("College", collegeId);

        var tpo = await _tpoRepository.GetPrimaryTpoByCollegeIdAsync(collegeId, ct)
            ?? throw new NotFoundException(
                $"No primary TPO is assigned to college '{college.Name}'.");

        if (tpo.TpoId != userId)
            throw new BusinessRuleException("The specified user is not the primary TPO of this college.");

        _tpoRepository.Update(tpo);

        await _tpoRepository.SaveChangesAsync(ct);

        // var events = college.DomainEvents.ToList();
        // await _eventPublisher.PublishAsync(events, ct);
        // college.ClearDomainEvents();

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
            VerificationStatus = tpo.VerificationStatus.ToString(),
            CreatedAt = tpo.CreatedAt
        };
}