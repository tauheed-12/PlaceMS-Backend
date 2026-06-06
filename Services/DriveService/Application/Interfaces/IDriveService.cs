// Application/Interfaces/IDriveService.cs
using DriveService.Application.DTOs.Requests;
using DriveService.Application.DTOs.Responses;
using SharedKernel.Models;

namespace DriveService.Application.Interfaces;

public interface IDriveService
{
    // ── Recruiter ─────────────────────────────────────────────────

    Task<CreateDriveResponse> CreateDriveAsync(
        Guid recruiterUserId,
        CreateDriveRequest request,
        CancellationToken ct = default);

    Task<PagedResult<DriveListItemResponse>> GetRecruiterDrivesAsync(
        Guid recruiterUserId,
        DriveListQuery query,
        CancellationToken ct = default);

    Task<DriveDetailResponse> GetDriveDetailAsync(
        Guid driveId,
        Guid callerUserId,
        CancellationToken ct = default);

    Task<DriveDetailResponse> UpdateDriveAsync(
        Guid driveId,
        Guid recruiterUserId,
        UpdateDriveRequest request,
        CancellationToken ct = default);

    Task DeactivateDriveAsync(
        Guid driveId,
        Guid requesterUserId,
        string requesterRole,
        CancellationToken ct = default);

    Task ResubmitToCollegeAsync(
        Guid driveId,
        Guid collegeId,
        Guid recruiterUserId,
        CancellationToken ct = default);

    // ── TPO ───────────────────────────────────────────────────────

    Task<PagedResult<TpoDriveResponse>> GetCollegeDrivesAsync(
        Guid collegeId,
        DriveListQuery query,
        CancellationToken ct = default);

    Task<PagedResult<TpoDriveResponse>> GetPendingCollegeDrivesAsync(
        Guid collegeId,
        DriveListQuery query,
        CancellationToken ct = default);

    Task<DriveApprovalActionResponse> ApproveDriveAsync(
        Guid driveId,
        Guid collegeId,
        Guid tpoUserId,
        ApproveDriveRequest request,
        CancellationToken ct = default);

    Task<DriveApprovalActionResponse> RejectDriveAsync(
        Guid driveId,
        Guid collegeId,
        Guid tpoUserId,
        RejectDriveRequest request,
        CancellationToken ct = default);

    Task<DriveApprovalActionResponse> RequestChangesAsync(
        Guid driveId,
        Guid collegeId,
        Guid tpoUserId,
        RequestChangesRequest request,
        CancellationToken ct = default);

    // ── Student ───────────────────────────────────────────────────

    Task<PagedResult<StudentDriveResponse>> GetAvailableDrivesAsync(
        Guid collegeId,
        AvailableDrivesQuery query,
        CancellationToken ct = default);

    Task<StudentDriveResponse> GetStudentDriveDetailAsync(
        Guid driveId,
        Guid collegeId,
        CancellationToken ct = default);

    // ── Admin ─────────────────────────────────────────────────────

    Task<PagedResult<DriveListItemResponse>> GetAllDrivesAsync(
        AdminDriveListQuery query,
        CancellationToken ct = default);

    // ── Internal ──────────────────────────────────────────────────

    Task<InternalDriveDetailResponse> GetInternalDriveDetailAsync(
        Guid driveId,
        CancellationToken ct = default);

    Task<DriveCollegeStatusResponse> GetDriveCollegeStatusAsync(
        Guid driveId,
        Guid collegeId,
        CancellationToken ct = default);
}