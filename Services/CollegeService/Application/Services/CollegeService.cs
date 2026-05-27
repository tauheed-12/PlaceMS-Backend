using SharedKernel.Exceptions;
using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Domain.Entities;
using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.Interfaces.Repositories;
using CollegeService.Application.Interfaces;

namespace CollegeService.Application.Services;

public class CollegeService : ICollegeService
{
    private readonly ICollegeRepository _collegeRepo;
    private readonly ILogger<CollegeService> _logger;
    private readonly IDomainEventPublisher _eventPublisher;

    public CollegeService(ICollegeRepository collegeRepo, ILogger<CollegeService> logger, IDomainEventPublisher eventPublisher)
    {
        _collegeRepo = collegeRepo;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateCollegeResponseDto> RegisterAsync(CreateCollegeRequestDto request, Guid registeredBy, CancellationToken ct)
    {
        if (await _collegeRepo.EmailExistsAsync(request.Email, ct))
            throw new ConflictException("college", "email", request.Email);

        if (await _collegeRepo.CodeExistsAsync(request.Code, ct))
            throw new ConflictException("college", "code", request.Code);

        var college = College.Create(
            request.Name,
            request.Code,
            request.Email,
            request.Phone,
            request.Website,
            request.AffiliatedBy,
            request.Type,
            request.City,
            request.State,
            request.Pincode,
            registeredBy
            );

        await _collegeRepo.AddAsync(college, ct);

        var events = college.DomainEvents.ToList();
        await _collegeRepo.SaveChangesAsync(ct);
        await _eventPublisher.PublishAsync(events, ct);
        college.ClearDomainEvents();

        _logger.LogInformation("College {CollegeId} registered with name {Name}", college.Id, request.Name);

        return BuildCollegeResponse(college);
    }

    public async Task<UpdateCollegeResponseDto> UpdateAsync(UpdateCollegeRequestDto request, Guid updatedBy, CancellationToken ct)
    {
        var existingCollege = await _collegeRepo.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("College not found");

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            existingCollege.Email != request.Email &&
            await _collegeRepo.EmailExistsAsync(request.Email, ct))
        {
            throw new ConflictException("college", "email", request.Email);
        }

        if (!string.IsNullOrWhiteSpace(request.Code) &&
            existingCollege.Code != request.Code &&
            await _collegeRepo.CodeExistsAsync(request.Code, ct))
        {
            throw new ConflictException("college", "code", request.Code);
        }

        existingCollege.UpdateDetails(
            name: request.Name,
            code: request.Code,
            email: request.Email,
            phone: request.Phone,
            website: request.Website,
            affiliatedBy: request.AffiliatedBy,
            type: request.Type,
            city: request.City,
            state: request.State,
            pincode: request.Pincode,
            updatedBy: updatedBy
        );

        await _collegeRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "College {CollegeId} updated",
            existingCollege.Id);

        return new UpdateCollegeResponseDto
        {
            Message = "College updated successfully",
            College = new CollegeShortDto
            {
                Id = existingCollege.Id,
                Name = existingCollege.Name,
                Code = existingCollege.Code,
                City = existingCollege.City,
                State = existingCollege.State,
                AccountStatus = existingCollege.AccountStatus,
            }
        };
    }

    public async Task DeactivateCollegeAsync(Guid collegeId, Guid deactivatedBy, CancellationToken ct)
    {
        var existingCollege = await _collegeRepo.GetByIdAsync(collegeId, ct) ??
            throw new NotFoundException("College not found");

        existingCollege.Deactivate(deactivatedBy);

        var events = existingCollege.DomainEvents.ToList();
        await _collegeRepo.SaveChangesAsync(ct);
        await _eventPublisher.PublishAsync(events, ct);
        existingCollege.ClearDomainEvents();

        _logger.LogInformation("College {CollegeId} deactivated successfully", existingCollege.Id);
    }


    public async Task ReactivateCollegeAsync(Guid collegeId, Guid activatedBy, CancellationToken ct)
    {
        var existingCollege = await _collegeRepo.GetByIdAsync(collegeId, ct) ??
            throw new NotFoundException("College not found");

        existingCollege.Reactivate(activatedBy);

        var events = existingCollege.DomainEvents.ToList();
        await _collegeRepo.SaveChangesAsync(ct);
        await _eventPublisher.PublishAsync(events, ct);
        existingCollege.ClearDomainEvents();

        _logger.LogInformation("College {CollegeId} reactivated successfully", existingCollege.Id);
    }


    private static CreateCollegeResponseDto BuildCollegeResponse(College college)
        => new()
        {
            Message = "College resgistered successfully",
            College = new()
            {
                Name = college.Name,
                Code = college.Code,
                City = college.City,
                State = college.State,
                AccountStatus = college.AccountStatus,
                HasTpoAssigned = false,
            }
        };
}