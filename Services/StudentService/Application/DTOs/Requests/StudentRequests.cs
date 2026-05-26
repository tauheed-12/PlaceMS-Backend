namespace StudentService.Application.DTOs.Requests;

public record UpdatePersonalInfoRequest
{
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Branch { get; init; } = string.Empty;
    public int BatchYear { get; init; }
    public double Cgpa { get; init; }
    public string? AboutMe { get; init; }
}

public record AddEducationRequest
{
    public string Degree { get; init; } = string.Empty;
    public string Institution { get; init; } = string.Empty;
    public int StartYear { get; init; }
    public int? EndYear { get; init; }
    public string? Score { get; init; }
}

public record UpdateEducationRequest
{
    public string Degree { get; init; } = string.Empty;
    public string Institution { get; init; } = string.Empty;
    public int StartYear { get; init; }
    public int? EndYear { get; init; }
    public string? Score { get; init; }
}

public record ReplaceSkillsRequest
{
    public List<string> Skills { get; init; } = new();
}

public record AddProjectRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> TechStack { get; init; } = new();
    public string? ProjectUrl { get; init; }
}

public record UpdateProjectRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> TechStack { get; init; } = new();
    public string? ProjectUrl { get; init; }
}

public record AddCertificationRequest
{
    public string Title { get; init; } = string.Empty;
    public string IssuingOrganization { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CredentialUrl { get; init; }
}

public record UpdateCertificationRequest
{
    public string Title { get; init; } = string.Empty;
    public string IssuingOrganization { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CredentialUrl { get; init; }
}

/// <summary>Used by Kafka consumer to create the skeleton profile.</summary>
public record CreateStudentProfileRequest
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeCode { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
}