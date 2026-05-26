using SharedKernel.Abstractions;
using SharedKernel.Exceptions;
using StudentService.Domain.Events;

public class ResumeFile : BaseEntity
{
    public Guid StudentProfileId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredObjectName { get; private set; } = string.Empty;  // MinIO object key
    public string BucketName { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = false;
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    private ResumeFile() { }

    public static ResumeFile Create(
        Guid studentProfileId,
        string originalFileName,
        string storedObjectName,
        string bucketName,
        long fileSizeBytes,
        string contentType)
    {
        if (fileSizeBytes > 5 * 1024 * 1024)  // 5 MB limit
            throw new DomainValidationException("Resume file size must not exceed 5MB.");

        if (contentType != "application/pdf")
            throw new DomainValidationException("Only PDF resumes are accepted.");

        return new ResumeFile
        {
            StudentProfileId = studentProfileId,
            OriginalFileName = originalFileName,
            StoredObjectName = storedObjectName,
            BucketName = bucketName,
            FileSizeBytes = fileSizeBytes,
            ContentType = contentType,
            IsActive = false
        };
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}