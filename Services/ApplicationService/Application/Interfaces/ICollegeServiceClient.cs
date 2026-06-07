namespace ApplicationService.Application.Interfaces;

public interface ICollegeServiceClient
{
    Task<CollegeDetail?> GetCollegeDetailAsync(Guid collegeId, CancellationToken ct = default);
}

public record CollegeDetail
{
    public Guid CollegeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}
