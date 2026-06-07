using ApplicationService.Application.DTOs.Requests;
using ApplicationService.Application.DTOs.Responses;
using ApplicationService.Application.Interfaces;
using ApplicationService.Domain.Entities;
using SharedKernel.Enums;
using SharedKernel.Exceptions;
using SharedKernel.Models;

namespace ApplicationService.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repository;
    private readonly IStudentServiceClient _studentClient;
    private readonly IDriveServiceClient _driveClient;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        IApplicationRepository repository,
        IStudentServiceClient studentClient,
        IDriveServiceClient driveClient,
        ILogger<ApplicationService> logger)
    {
        _repository = repository;
        _studentClient = studentClient;
        _driveClient = driveClient;
        _logger = logger;
    }

    public async Task<CreateApplicationResponseDto> ApplyAsync(Guid studentId, CreateApplicationRequestDto request, CancellationToken ct = default)
    {
        if (studentId == Guid.Empty)
            throw new DomainValidationException("StudentId is required.");

        if (request.DriveId == Guid.Empty)
            throw new DomainValidationException("DriveId is required.");

        if (await _repository.HasAppliedAsync(studentId, request.DriveId, ct))
            throw new ConflictException("Application", "drive/student", request.DriveId);

        var eligibility = await _studentClient.GetEligibilityAsync(studentId, ct)
            ?? throw new NotFoundException("Student eligibility", studentId);

        if (!eligibility.HasActiveResume)
            throw new ForbiddenException("Active resume is required before applying to a drive.");

        var drive = await _driveClient.GetInternalDriveDetailAsync(request.DriveId, ct)
            ?? throw new NotFoundException("Drive", request.DriveId);

        if (drive.IsDeactivated)
            throw new ForbiddenException("This drive is no longer accepting applications.");

        if (drive.ApplicationDeadline < DateTime.UtcNow)
            throw new ForbiddenException("The application deadline has already passed.");

        if (eligibility.BatchYear != drive.EligibleBatch)
            throw new ForbiddenException("Your batch is not eligible for this drive.");

        var branch = ParseEligibleBranch(eligibility.Branch);
        if (branch == EligibleBranch.None || !drive.EligibleBranches.HasFlag(branch))
            throw new ForbiddenException("Your branch is not eligible for this drive.");

        var collegeStatus = await _driveClient.GetDriveCollegeStatusAsync(request.DriveId, eligibility.CollegeId, ct)
            ?? throw new NotFoundException("Drive college status", $"{request.DriveId}/{eligibility.CollegeId}");

        if (!collegeStatus.CanApply)
            throw new ForbiddenException("Your college is not approved to apply for this drive.");

        var summary = await _studentClient.GetStudentSummaryAsync(studentId, ct)
            ?? throw new NotFoundException("Student summary", studentId);

        var application = StudentApplication.Create(
            request.DriveId,
            eligibility.CollegeId,
            studentId,
            summary.FullName,
            summary.Email,
            summary.CollegeName,
            drive.CompanyName,
            drive.JobRole);

        await _repository.AddApplicationAsync(application, ct);

        return new CreateApplicationResponseDto
        {
            ApplicationId = application.Id,
            Status = application.Status,
            AppliedOn = application.AppliedOn,
            Message = "Application submitted successfully."
        };
    }

    public async Task<WithdrawApplicationResponseDto> WithdrawAsync(Guid applicationId, Guid studentId, CancellationToken ct = default)
    {
        var application = await _repository.GetApplicationByIdAsync(applicationId, ct)
            ?? throw new NotFoundException("Application", applicationId);

        if (application.StudentId != studentId)
            throw new ForbiddenException("You can only withdraw your own application.");

        application.Withdraw();
        await _repository.UpdateApplicationAsync(application, ct);

        return new WithdrawApplicationResponseDto
        {
            ApplicationId = application.Id,
            Status = application.Status,
            UpdatedOn = application.UpdatedAt,
            Message = "Application withdrawn successfully."
        };
    }

    public async Task<UpdateApplicationStatusResponseDto> UpdateStatusAsync(Guid applicationId, UpdateApplicationStatusRequestDto request, CancellationToken ct = default)
    {
        var application = await _repository.GetApplicationByIdAsync(applicationId, ct)
            ?? throw new NotFoundException("Application", applicationId);

        application.UpdateStatus(request.Status);
        await _repository.UpdateApplicationAsync(application, ct);

        return new UpdateApplicationStatusResponseDto
        {
            ApplicationId = application.Id,
            Status = application.Status,
            UpdatedOn = application.UpdatedAt,
            Message = "Application status updated successfully."
        };
    }

    public async Task<PagedResult<StudentApplicationDto>> GetByStudentIdAsync(Guid studentId, ApplicationFilterRequestDto request, CancellationToken ct = default)
    {
        var page = await _repository.GetApplicationsByStudentIdAsync(studentId, request.PageNumber, request.PageSize, ct);
        return PagedResult<StudentApplicationDto>.Create(page.Items.Select(MapToStudentApplicationDto).ToList(), page.TotalCount, page.PageNumber, page.PageSize);
    }

    public async Task<PagedResult<ApplicationDetailsDto>> GetByDriveIdAsync(Guid driveId, ApplicationFilterRequestDto request, CancellationToken ct = default)
    {
        var page = await _repository.GetApplicationsByDriveIdAsync(driveId, request.PageNumber, request.PageSize, request.Search, request.Status, ct);
        return PagedResult<ApplicationDetailsDto>.Create(page.Items.Select(MapToApplicationDetailsDto).ToList(), page.TotalCount, page.PageNumber, page.PageSize);
    }

    public async Task<PagedResult<ApplicationDetailsDto>> GetByCollegeIdAsync(Guid collegeId, ApplicationFilterRequestDto request, CancellationToken ct = default)
    {
        var page = await _repository.GetApplicationsByCollegeIdAsync(collegeId, request.PageNumber, request.PageSize, request.Search, request.Status, ct);
        return PagedResult<ApplicationDetailsDto>.Create(page.Items.Select(MapToApplicationDetailsDto).ToList(), page.TotalCount, page.PageNumber, page.PageSize);
    }

    public async Task<ApplicationShortDto?> GetByStudentAndDriveAsync(Guid studentId, Guid driveId, CancellationToken ct = default)
    {
        var application = await _repository.GetApplicationByStudentAndDriveAsync(studentId, driveId, ct);
        return application is null ? null : MapToApplicationShortDto(application);
    }

    public async Task<ApplicationStatisticsDto> GetDriveStatisticsAsync(Guid driveId, CancellationToken ct = default)
    {
        var counts = await _repository.GetStatusCountsForDriveAsync(driveId, ct);
        return BuildStatistics(counts);
    }

    public async Task<ApplicationStatisticsDto> GetCollegeStatisticsAsync(Guid collegeId, CancellationToken ct = default)
    {
        var counts = await _repository.GetStatusCountsForCollegeAsync(collegeId, ct);
        return BuildStatistics(counts);
    }

    public async Task<ApplicationStatisticsDto> GetStudentStatisticsAsync(Guid studentId, CancellationToken ct = default)
    {
        var counts = await _repository.GetStatusCountsForStudentAsync(studentId, ct);
        return BuildStatistics(counts);
    }

    public async Task<bool> HasAppliedAsync(Guid studentId, Guid driveId, CancellationToken ct = default)
    {
        return await _repository.HasAppliedAsync(studentId, driveId, ct);
    }

    public async Task<bool> ExistsAsync(Guid applicationId, CancellationToken ct = default)
    {
        return await _repository.ExistsAsync(applicationId, ct);
    }

    public async Task<bool> IsOwnerAsync(Guid applicationId, Guid studentId, CancellationToken ct = default)
    {
        var application = await _repository.GetApplicationByIdAsync(applicationId, ct);
        return application is not null && application.StudentId == studentId;
    }

    public Task BulkShortlistAsync() => throw new NotImplementedException("Bulk shortlist is not implemented yet.");
    public Task BulkRejectAsync() => throw new NotImplementedException("Bulk reject is not implemented yet.");
    public Task BulkUpdateStatusAsync() => throw new NotImplementedException("Bulk status update is not implemented yet.");
    public Task ExportApplicationsAsync() => throw new NotImplementedException("Application export is not implemented yet.");

    private static StudentApplicationDto MapToStudentApplicationDto(StudentApplication application)
        => new()
        {
            ApplicationId = application.Id,
            DriveId = application.DriveId,
            CompanyName = application.CompanyName,
            JobRole = application.JobRole,
            Status = application.Status,
            AppliedOn = application.AppliedOn
        };

    private static ApplicationDetailsDto MapToApplicationDetailsDto(StudentApplication application)
        => new()
        {
            ApplicationId = application.Id,
            DriveId = application.DriveId,
            StudentId = application.StudentId,
            StudentName = application.StudentName,
            StudentEmail = application.StudentEmail,
            CollegeName = application.CollegeName,
            Status = application.Status,
            AppliedOn = application.AppliedOn
        };

    private static ApplicationShortDto MapToApplicationShortDto(StudentApplication application)
        => new()
        {
            ApplicationId = application.Id,
            StudentId = application.StudentId,
            StudentName = application.StudentName,
            CollegeName = application.CollegeName,
            Status = application.Status,
            AppliedOn = application.AppliedOn
        };

    private static ApplicationStatisticsDto BuildStatistics(Dictionary<ApplicationStatus, int> counts)
    {
        counts = Enum.GetValues<ApplicationStatus>().ToDictionary(status => status, status => counts.TryGetValue(status, out var value) ? value : 0);

        return new ApplicationStatisticsDto
        {
            TotalApplications = counts.Values.Sum(),
            TotalApplied = counts.GetValueOrDefault(ApplicationStatus.Applied),
            TotalUnderReview = counts.GetValueOrDefault(ApplicationStatus.UnderReview),
            TotalShortlisted = counts.GetValueOrDefault(ApplicationStatus.Shortlisted),
            TotalOffered = counts.GetValueOrDefault(ApplicationStatus.Offered),
            TotalAccepted = counts.GetValueOrDefault(ApplicationStatus.Accepted),
            TotalRejected = counts.GetValueOrDefault(ApplicationStatus.Rejected),
            TotalWithdrawn = counts.GetValueOrDefault(ApplicationStatus.Withdrawn)
        };
    }

    private static EligibleBranch ParseEligibleBranch(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
            return EligibleBranch.None;

        if (Enum.TryParse<EligibleBranch>(branchName.Replace(" ", string.Empty), true, out var parsed))
            return parsed;

        branchName = branchName.Trim().ToLowerInvariant();

        return branchName switch
        {
            "computer science" or "cse" => EligibleBranch.ComputerScience,
            "information technology" or "it" => EligibleBranch.InformationTechnology,
            "electronics and communication" or "ece" => EligibleBranch.ElectronicsAndCommunication,
            "electrical engineering" or "ee" => EligibleBranch.ElectricalEngineering,
            "mechanical engineering" or "me" => EligibleBranch.MechanicalEngineering,
            "civil engineering" or "ce" => EligibleBranch.CivilEngineering,
            _ => EligibleBranch.None
        };
    }
}
