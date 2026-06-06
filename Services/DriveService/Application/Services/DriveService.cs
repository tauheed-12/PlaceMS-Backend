using DriveService.Application.DTOs.Requests;
using DriveService.Application.DTOs.Responses;
using DriveService.Application.Interfaces;
using DriveService.Domain.Entities;

namespace DriveService.Application.Services;

public class DriveService : IDriveService
{
    private readonly IDriveRepository _driveRepo;
    private readonly ILogger<DriveService> _logger;

    public DriveService(IDriveRepository driveRepo, ILogger<DriveService> logger)
    {
        _driveRepo = driveRepo;
        _logger = logger;
    }

    public async Task<CreateDriveResponse> CreateDriveAsync(Guid recruiterUserId, CreateDriveRequest request, CancellationToken ct = default)
    {
        var drive = Drive.Create(
            recruiterUserId: recruiterUserId,
            companyName: request.CompanyName.Trim(),
            jobRole: request.JobRole.Trim(),
            jobDescription: request.JobDescription.Trim(),
            ctc: request.CTC.Trim(),
            location: request.Location.Trim(),
            employmentType: request.EmploymentType,
            driveDate: request.DriveDate,
            applicationDeadline: request.ApplicationDeadline,
            minCgpa: request.MinCgpa,
            eligibleBranches: request.EligibleBranches,
            eligibleBatch: request.EligibleBatch
        );

        await _driveRepo.AddAsync(drive, ct);
        await _driveRepo.SaveChangesAsync(ct);

        return new CreateDriveResponse
        {
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            Message = "Drive Created Successfully"
        };
    }
}