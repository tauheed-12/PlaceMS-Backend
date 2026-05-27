
namespace StudentService.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string objectName, string bucketName, string contentType, CancellationToken ct = default);
    Task<string> GeneratePresignedUrlAsync(string objectName, string bucketName, int expiryMinutes = 60, CancellationToken ct = default);
    Task DeleteAsync(string objectName, string bucketName, CancellationToken ct = default);
    Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct = default);
}