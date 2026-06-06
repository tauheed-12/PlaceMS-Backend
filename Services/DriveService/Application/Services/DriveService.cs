using DriveService.Application.DTOs.Requests;
using DriveService.Application.DTOs.Responses;
using DriveService.Application.Interfaces;
using DriveService.Domain.Entities;
using SharedKernel.Enums;
using SharedKernel.Exceptions;
using SharedKernel.Models;

namespace DriveService.Application.Services;

public class DriveService : IDriveService
{
    private readonly IDriveRepository _driveRepo;
    private readonly ICollegeServiceClient _collegeClient;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly ILogger<DriveService> _logger;

    public DriveService(
        IDriveRepository driveRepo,
        ICollegeServiceClient collegeClient,
        IDomainEventPublisher eventPublisher,
        ILogger<DriveService> logger)
    {
        _driveRepo = driveRepo;
        _collegeClient = collegeClient;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CreateDriveResponse> CreateDriveAsync(Guid recruiterUserId, CreateDriveRequest request, CancellationToken ct = default)
    {
        if (request.TargetCollegeIds == null || !request.TargetCollegeIds.Any())
            throw new DomainValidationException("At least one target college must be provided.");

        if (request.Rounds == null || !request.Rounds.Any())
            throw new DomainValidationException("At least one interview round must be specified.");

        var collegeInfos = await _collegeClient.GetCollegesInfoAsync(request.TargetCollegeIds, ct);

        var missingCollegeIds = request.TargetCollegeIds.Except(collegeInfos.Select(c => c.CollegeId)).ToList();
        if (missingCollegeIds.Any())
            throw new NotFoundException("One or more target colleges were not found or are inactive.");

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
            eligibleBatch: request.EligibleBatch,
            rounds: request.Rounds,
            colleges: collegeInfos.Select(c => (
                CollegeId: c.CollegeId,
                CollegeName: c.CollegeName,
                TpoUserId: c.TpoUserId,
                TpoEmail: c.TpoEmail,
                TpoName: c.TpoName)).ToList());

        await _driveRepo.AddAsync(drive, ct);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);

        _logger.LogInformation("Created drive {DriveId} for recruiter {RecruiterUserId}", drive.Id, recruiterUserId);

        return new CreateDriveResponse
        {
            DriveId = drive.Id,
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            CollegesNotified = drive.DriveColleges.Count,
            Message = "Drive created successfully"
        };
    }

    public async Task<PagedResult<DriveListItemResponse>> GetRecruiterDrivesAsync(Guid recruiterUserId, DriveListQuery query, CancellationToken ct = default)
    {
        var result = await _driveRepo.GetByRecruiterAsync(
            recruiterUserId,
            query.Page,
            query.PageSize,
            query.Search,
            query.IsDeactivated,
            ct);

        return PagedResult<DriveListItemResponse>.Create(
            result.Items.Select(MapToDriveListItem).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task<DriveDetailResponse> GetDriveDetailAsync(Guid driveId, Guid callerUserId, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        return MapToDriveDetail(drive);
    }

    public async Task<DriveDetailResponse> UpdateDriveAsync(Guid driveId, Guid recruiterUserId, UpdateDriveRequest request, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        if (drive.RecruiterUserId != recruiterUserId)
            throw new ForbiddenException("You can only update your own drive.");

        drive.Update(
            request.JobRole.Trim(),
            request.JobDescription.Trim(),
            request.CTC.Trim(),
            request.Location.Trim(),
            request.EmploymentType,
            request.DriveDate,
            request.ApplicationDeadline,
            request.MinCgpa,
            request.EligibleBranches,
            request.EligibleBatch,
            request.Rounds);

        _driveRepo.Update(drive);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);

        return MapToDriveDetail(drive);
    }

    public async Task DeactivateDriveAsync(Guid driveId, Guid requesterUserId, string requesterRole, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        var isAdmin = requesterRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true ||
                      requesterRole?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true;

        if (!isAdmin && drive.RecruiterUserId != requesterUserId)
            throw new ForbiddenException("You can only deactivate your own drive.");

        drive.Deactivate();
        _driveRepo.Update(drive);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);
    }

    public async Task ResubmitToCollegeAsync(Guid driveId, Guid collegeId, Guid recruiterUserId, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        if (drive.RecruiterUserId != recruiterUserId)
            throw new ForbiddenException("You can only resubmit your own drive.");

        drive.ResubmitToCollege(collegeId, recruiterUserId);
        _driveRepo.Update(drive);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);
    }

    public async Task<PagedResult<TpoDriveResponse>> GetCollegeDrivesAsync(Guid collegeId, DriveListQuery query, CancellationToken ct = default)
    {
        var result = await _driveRepo.GetByCollegeAsync(collegeId, query.Page, query.PageSize, query.Search, ct);

        return PagedResult<TpoDriveResponse>.Create(
            result.Items.Select(d => MapToTpoDrive(d, collegeId)).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task<PagedResult<TpoDriveResponse>> GetPendingCollegeDrivesAsync(Guid collegeId, DriveListQuery query, CancellationToken ct = default)
    {
        var result = await _driveRepo.GetPendingByCollegeAsync(collegeId, query.Page, query.PageSize, ct);

        return PagedResult<TpoDriveResponse>.Create(
            result.Items.Select(d => MapToTpoDrive(d, collegeId)).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task<DriveApprovalActionResponse> ApproveDriveAsync(Guid driveId, Guid collegeId, Guid tpoUserId, ApproveDriveRequest request, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        drive.Approve(collegeId, tpoUserId, request.Note);
        _driveRepo.Update(drive);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);

        var college = drive.DriveColleges.First(dc => dc.CollegeId == collegeId);
        return MapToApprovalActionResponse(driveId, collegeId, college);
    }

    public async Task<DriveApprovalActionResponse> RejectDriveAsync(Guid driveId, Guid collegeId, Guid tpoUserId, RejectDriveRequest request, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        drive.Reject(collegeId, tpoUserId, request.Note);
        _driveRepo.Update(drive);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);

        var college = drive.DriveColleges.First(dc => dc.CollegeId == collegeId);
        return MapToApprovalActionResponse(driveId, collegeId, college);
    }

    public async Task<DriveApprovalActionResponse> RequestChangesAsync(Guid driveId, Guid collegeId, Guid tpoUserId, RequestChangesRequest request, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        drive.RequestChanges(collegeId, tpoUserId, request.Note);
        _driveRepo.Update(drive);
        await _driveRepo.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(drive, ct);

        var college = drive.DriveColleges.First(dc => dc.CollegeId == collegeId);
        return MapToApprovalActionResponse(driveId, collegeId, college);
    }

    public async Task<PagedResult<StudentDriveResponse>> GetAvailableDrivesAsync(Guid collegeId, AvailableDrivesQuery query, CancellationToken ct = default)
    {
        var result = await _driveRepo.GetAvailableForCollegeAsync(
            collegeId,
            query.Page,
            query.PageSize,
            query.Search,
            query.EmploymentType?.ToString(),
            ct);

        return PagedResult<StudentDriveResponse>.Create(
            result.Items.Select(MapToStudentDrive).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task<StudentDriveResponse> GetStudentDriveDetailAsync(Guid driveId, Guid collegeId, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdWithAllAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        if (!drive.IsApprovedForCollege(collegeId) || drive.IsDeactivated || drive.IsDeadlinePassed())
            throw new NotFoundException("Drive is not available for this college.");

        return MapToStudentDrive(drive);
    }

    public async Task<PagedResult<DriveListItemResponse>> GetAllDrivesAsync(AdminDriveListQuery query, CancellationToken ct = default)
    {
        var result = await _driveRepo.GetAllAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.CollegeId,
            query.RecruiterUserId,
            query.IsDeactivated,
            ct);

        return PagedResult<DriveListItemResponse>.Create(
            result.Items.Select(MapToDriveListItem).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task<InternalDriveDetailResponse> GetInternalDriveDetailAsync(Guid driveId, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        return new InternalDriveDetailResponse
        {
            Id = drive.Id,
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            CTC = drive.CTC,
            MinCgpa = drive.MinCgpa,
            EligibleBranches = drive.EligibleBranches,
            EligibleBatch = drive.EligibleBatch,
            ApplicationDeadline = drive.ApplicationDeadline,
            IsDeactivated = drive.IsDeactivated,
            RecruiterUserId = drive.RecruiterUserId
        };
    }

    public async Task<DriveCollegeStatusResponse> GetDriveCollegeStatusAsync(Guid driveId, Guid collegeId, CancellationToken ct = default)
    {
        var drive = await _driveRepo.GetByIdAsync(driveId, ct)
            ?? throw new NotFoundException("Drive", driveId);

        var driveCollege = await _driveRepo.GetDriveCollegeAsync(driveId, collegeId, ct)
            ?? throw new NotFoundException("Drive college status not found.");

        var isApproved = driveCollege.ApprovalStatus == DriveApprovalStatus.Approved;
        var deadlinePassed = drive.IsDeadlinePassed();

        return new DriveCollegeStatusResponse
        {
            DriveId = driveId,
            CollegeId = collegeId,
            IsApproved = isApproved,
            IsDeactivated = drive.IsDeactivated,
            IsDeadlinePassed = deadlinePassed,
            CanApply = isApproved && !drive.IsDeactivated && !deadlinePassed
        };
    }

    private static DriveListItemResponse MapToDriveListItem(Drive drive)
        => new()
        {
            Id = drive.Id,
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            CTC = drive.CTC,
            Location = drive.Location,
            EmploymentType = drive.EmploymentType.ToString(),
            ApplicationDeadline = drive.ApplicationDeadline,
            MinCgpa = drive.MinCgpa,
            IsDeactivated = drive.IsDeactivated,
            TotalCollegesTargeted = drive.DriveColleges.Count,
            ApprovedCollegesCount = drive.DriveColleges.Count(dc => dc.ApprovalStatus == DriveApprovalStatus.Approved),
            PendingCollegesCount = drive.DriveColleges.Count(dc => dc.ApprovalStatus == DriveApprovalStatus.Pending),
            RejectedCollegesCount = drive.DriveColleges.Count(dc => dc.ApprovalStatus == DriveApprovalStatus.Rejected),
            ChangesRequestedCount = drive.DriveColleges.Count(dc => dc.ApprovalStatus == DriveApprovalStatus.ChangesRequested),
            CreatedAt = drive.CreatedAt
        };

    private static DriveDetailResponse MapToDriveDetail(Drive drive)
        => new()
        {
            Id = drive.Id,
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            JobDescription = drive.JobDescription,
            CTC = drive.CTC,
            Location = drive.Location,
            EmploymentType = drive.EmploymentType.ToString(),
            DriveDate = drive.DriveDate,
            ApplicationDeadline = drive.ApplicationDeadline,
            MinCgpa = drive.MinCgpa,
            EligibleBranches = drive.EligibleBranches.ToString(),
            EligibleBatch = drive.EligibleBatch,
            IsDeactivated = drive.IsDeactivated,
            CanEdit = drive.CanEdit(),
            Rounds = drive.DriveRounds.OrderBy(r => r.RoundNumber).Select(r => r.RoundName).ToList(),
            Colleges = drive.DriveColleges.Select(dc => new DriveCollegeResponse
            {
                CollegeId = dc.CollegeId,
                CollegeName = dc.CollegeName,
                ApprovalStatus = dc.ApprovalStatus.ToString(),
                TpoNote = dc.TpoNote,
                ReviewedAt = dc.ReviewedAt
            }).ToList(),
            RecruiterUserId = drive.RecruiterUserId,
            CreatedAt = drive.CreatedAt,
            UpdatedAt = drive.UpdatedAt
        };

    private static TpoDriveResponse MapToTpoDrive(Drive drive, Guid collegeId)
    {
        var dc = drive.DriveColleges.First(c => c.CollegeId == collegeId);
        return new TpoDriveResponse
        {
            Id = drive.Id,
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            JobDescription = drive.JobDescription,
            CTC = drive.CTC,
            Location = drive.Location,
            EmploymentType = drive.EmploymentType.ToString(),
            DriveDate = drive.DriveDate,
            ApplicationDeadline = drive.ApplicationDeadline,
            MinCgpa = drive.MinCgpa,
            EligibleBranches = drive.EligibleBranches.ToString(),
            EligibleBatch = drive.EligibleBatch,
            Rounds = drive.DriveRounds.OrderBy(r => r.RoundNumber).Select(r => r.RoundName).ToList(),
            ApprovalStatus = dc.ApprovalStatus.ToString(),
            TpoNote = dc.TpoNote,
            ReviewedAt = dc.ReviewedAt,
            EligibleStudentCount = 0,
            ReceivedAt = drive.CreatedAt
        };
    }

    private static StudentDriveResponse MapToStudentDrive(Drive drive)
        => new()
        {
            Id = drive.Id,
            CompanyName = drive.CompanyName,
            JobRole = drive.JobRole,
            JobDescription = drive.JobDescription,
            CTC = drive.CTC,
            Location = drive.Location,
            EmploymentType = drive.EmploymentType.ToString(),
            DriveDate = drive.DriveDate,
            ApplicationDeadline = drive.ApplicationDeadline,
            MinCgpa = drive.MinCgpa,
            EligibleBranches = drive.EligibleBranches.ToString(),
            EligibleBatch = drive.EligibleBatch,
            Rounds = drive.DriveRounds.OrderBy(r => r.RoundNumber).Select(r => r.RoundName).ToList(),
            HasApplied = false,
            DaysUntilDeadline = Math.Max(0, (int)(drive.ApplicationDeadline - DateTime.UtcNow).TotalDays)
        };

    private async Task PublishDomainEventsAsync(Drive drive, CancellationToken ct)
    {
        var events = drive.DomainEvents.ToList();
        if (!events.Any())
            return;

        await _eventPublisher.PublishAsync(events, ct);
        drive.ClearDomainEvents();
    }

    private static DriveApprovalActionResponse MapToApprovalActionResponse(Guid driveId, Guid collegeId, DriveCollege driveCollege)
        => new()
        {
            DriveId = driveId,
            CollegeId = collegeId,
            NewStatus = driveCollege.ApprovalStatus.ToString(),
            Note = driveCollege.TpoNote,
            ReviewedAt = driveCollege.ReviewedAt ?? DateTime.UtcNow
        };
}
