using SharedKernel.Exceptions;

using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Domain.Entities;
using CollegeService.Application.Interfaces.Services;
using CollegeService.Application.Interfaces.Repositories;

namespace CollegeService.Application.Services;

public class CollegeService : ICollegeService
{
    private readonly ICollegeRepository _collegeRepo;
    private readonly ILogger<CollegeService> _logger;

    public CollegeService(ICollegeRepository collegeRepo, ILogger<CollegeService> logger)
    {
        _collegeRepo = collegeRepo;
        _logger = logger;
    }
    public async Task<CreateCollegeResponseDto> RegisterAsync(CreateCollegeRequestDto request, string registeredBy, CancellationToken ct)
    {
        string userName = "Hello"; // TODO: update it from jwt token

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
            userName
            );

        await _collegeRepo.AddAsync(college, ct);
        await _collegeRepo.SaveChangesAsync(ct);

        _logger.LogInformation("College {CollegeId} registered with name {Name}", college.Id, request.Name);

        return BuildCollegeResponse(college);
    }

    public async Task<UpdateCollegeResponseDto> UpdateAsync(UpdateCollegeRequestDto request, string updatedBy, CancellationToken ct)
    {
        string userName = "Hello"; // TODO: update it from jwt token

        var existingCollege = await _collegeRepo.GetByIdAsync(request.Id, ct) ??
            throw new NotFoundException("College not found");

        if (existingCollege.Email != request.Email && await _collegeRepo.EmailExistsAsync(request.Email, ct))
            throw new ConflictException("college", "email", request.Email);

        if (existingCollege.Code != request.Code && await _collegeRepo.CodeExistsAsync(request.Code, ct))
            throw new ConflictException("college", "code", request.Code);

        existingCollege.UpdateDetails(
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
            userName
        );

        _collegeRepo.Update(existingCollege);
        await _collegeRepo.SaveChangesAsync(ct);

        _logger.LogInformation("College {CollegeId} updated with name {Name}", existingCollege.Id, request.Name);

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

    public async Task DeactivateCollegeAsync(Guid collegeId, CancellationToken ct)
    {
        var existingCollege = await _collegeRepo.GetByIdAsync(collegeId, ct) ??
            throw new NotFoundException("College not found");

        existingCollege.Deactivate();
        await _collegeRepo.SaveChangesAsync(ct);
        _logger.LogInformation("College {CollegeId} deactivated successfully", existingCollege.Id);

    }

    public async Task ReactivateCollegeAsync(Guid collegeId, CancellationToken ct)
    {
        var existingCollege = await _collegeRepo.GetByIdAsync(collegeId, ct) ??
            throw new NotFoundException("College not found");

        existingCollege.Reactivate();
        await _collegeRepo.SaveChangesAsync(ct);
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