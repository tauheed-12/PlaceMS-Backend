using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using StudentService.Application.Interfaces;
using StudentService.Infrastructure.Settings;

namespace StudentService.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(IMinioClient minioClient, ILogger<MinioFileStorageService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string objectName,
        string bucketName,
        string contentType,
        CancellationToken ct = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(args, ct);
        _logger.LogInformation("Uploaded {ObjectName} to bucket {BucketName}", objectName, bucketName);
        return objectName;
    }

    public async Task<string> GeneratePresignedUrlAsync(
        string objectName,
        string bucketName,
        int expiryMinutes = 60,
        CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryMinutes * 60); // Convert to seconds

        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task DeleteAsync(string objectName, string bucketName, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        await _minioClient.RemoveObjectAsync(args, ct);
        _logger.LogInformation("Deleted {ObjectName} from bucket {BucketName}", objectName, bucketName);
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct = default)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await _minioClient.BucketExistsAsync(existsArgs, ct);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeArgs, ct);
            _logger.LogInformation("Created MinIO bucket: {BucketName}", bucketName);
        }
    }
}