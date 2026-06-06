// Application/Interfaces/ICollegeServiceClient.cs
namespace DriveService.Application.Interfaces;

public interface ICollegeServiceClient
{
    /// <summary>
    /// Validates a college exists and is active.
    /// Returns college info including the TPO's userId.
    /// Called at drive creation to populate DriveCollege records.
    /// </summary>
    Task<CollegeInfoResult?> GetCollegeInfoAsync(
        Guid collegeId,
        CancellationToken ct = default);

    /// <summary>
    /// Batch fetch college info for multiple colleges.
    /// Called once at drive creation — avoids N HTTP calls.
    /// </summary>
    Task<List<CollegeInfoResult>> GetCollegesInfoAsync(
        List<Guid> collegeIds,
        CancellationToken ct = default);
}

public record CollegeInfoResult
{
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public Guid? TpoUserId { get; init; }
    public string? TpoEmail { get; init; }
    public string? TpoName { get; init; }
    public bool IsActive { get; init; }
}