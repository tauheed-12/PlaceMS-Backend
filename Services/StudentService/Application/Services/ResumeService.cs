using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;
using StudentService.Domain.Entities;
using StudentService.Infrastructure.Settings;

namespace StudentService.Application.Services;

public class ResumeService : IResumeService
{
    private readonly IStudentRepository _repository;
    private readonly IFileStorageService _storage;
    private readonly MinioSettings _settings;
    private readonly ILogger<ResumeService> _logger;

    public ResumeService(
        IStudentRepository repository,
        IFileStorageService storage,
        IOptions<MinioSettings> settings,
        ILogger<ResumeService> logger)
    {
        _repository = repository;
        _storage = storage;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ResumeResponse> UploadResumeAsync(Guid userId, IFormFile file, CancellationToken ct = default)
    {
        // Validate file
        if (file is null || file.Length == 0)
            throw new DomainValidationException("Resume file is required.");

        if (file.ContentType != "application/pdf")
            throw new DomainValidationException("Only PDF files are accepted.");

        if (file.Length > 5 * 1024 * 1024)
            throw new DomainValidationException("Resume file must not exceed 5MB.");

        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        // Generate unique object name to avoid collisions
        var extension = Path.GetExtension(file.FileName);
        var objectName = $"resumes/{userId}/{Guid.NewGuid()}{extension}";

        await _storage.EnsureBucketExistsAsync(_settings.BucketName, ct);

        // Upload to MinIO
        using var stream = file.OpenReadStream();
        await _storage.UploadAsync(stream, objectName, _settings.BucketName, file.ContentType, ct);

        // Create domain entity
        var resumeFile = ResumeFile.Create(
            profile.Id,
            file.FileName,
            objectName,
            _settings.BucketName,
            file.Length,
            file.ContentType);

        // Domain sets active resume — deactivates previous
        profile.SetActiveResume(resumeFile);
        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Resume uploaded for student {UserId}: {ObjectName}", userId, objectName);

        var downloadUrl = await _storage.GeneratePresignedUrlAsync(
            objectName, _settings.BucketName, _settings.PresignedUrlExpiryMinutes, ct);

        return new ResumeResponse
        {
            ResumeFileId = resumeFile.Id,
            OriginalFileName = resumeFile.OriginalFileName,
            FileSizeBytes = resumeFile.FileSizeBytes,
            DownloadUrl = downloadUrl,
            UploadedAt = resumeFile.UploadedAt
        };
    }

    public async Task<ResumeResponse> GetResumeUrlAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        var activeResume = profile.ResumeFiles.FirstOrDefault(r => r.IsActive)
            ?? throw new NotFoundException("No active resume found. Please upload a resume.");

        var downloadUrl = await _storage.GeneratePresignedUrlAsync(
            activeResume.StoredObjectName,
            activeResume.BucketName,
            _settings.PresignedUrlExpiryMinutes,
            ct);

        return new ResumeResponse
        {
            ResumeFileId = activeResume.Id,
            OriginalFileName = activeResume.OriginalFileName,
            FileSizeBytes = activeResume.FileSizeBytes,
            DownloadUrl = downloadUrl,
            UploadedAt = activeResume.UploadedAt
        };
    }

    public async Task DeleteResumeAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        var activeResume = profile.ResumeFiles.FirstOrDefault(r => r.IsActive)
            ?? throw new NotFoundException("No active resume to delete.");

        // Remove from MinIO
        await _storage.DeleteAsync(activeResume.StoredObjectName, activeResume.BucketName, ct);

        // Update domain
        profile.RemoveResume();
        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Resume deleted for student {UserId}", userId);
    }
}