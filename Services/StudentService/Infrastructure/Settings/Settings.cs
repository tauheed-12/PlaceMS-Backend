namespace StudentService.Infrastructure.Settings;

public class MinioSettings
{
    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "pms-resumes";
    public bool UseSSL { get; init; } = false;
    public int PresignedUrlExpiryMinutes { get; init; } = 60;
}

public class KafkaSettings
{
    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "student-service";
}

public class ServiceUrlSettings
{
    public string CollegeService { get; init; } = string.Empty;
}