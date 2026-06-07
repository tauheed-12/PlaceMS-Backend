using SharedKernel.Enums;

namespace ApplicationService.Application.Interfaces;

public interface IStudentServiceClient
{
    Task<StudentEligibility?> GetEligibilityAsync(Guid studentId, CancellationToken ct = default);
    Task<StudentSummary?> GetStudentSummaryAsync(Guid studentId, CancellationToken ct = default);
}

public record StudentEligibility
{
    public Guid UserId { get; init; }
    public Guid CollegeId { get; init; }
    public string Branch { get; init; } = string.Empty;
    public int BatchYear { get; init; }
    public double Cgpa { get; init; }
    public bool HasActiveResume { get; init; }
    public int ProfileCompletionScore { get; init; }
}

public record StudentSummary
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public string Branch { get; init; } = string.Empty;
    public int BatchYear { get; init; }
    public double Cgpa { get; init; }
    public int ProfileCompletionScore { get; init; }
}
