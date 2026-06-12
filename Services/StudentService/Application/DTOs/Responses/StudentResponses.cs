namespace StudentService.Application.DTOs.Responses;

public record StudentProfileResponse
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string Branch { get; init; } = string.Empty;
    public int BatchYear { get; init; }
    public double Cgpa { get; init; }
    public string? AboutMe { get; init; }
    public int ProfileCompletionScore { get; init; }
    public bool HasActiveResume { get; init; }
    public List<EducationResponse> Education { get; init; } = new();
    public List<string> Skills { get; init; } = new();
    public List<ProjectResponse> Projects { get; init; } = new();
    public List<CertificationResponse> Certifications { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record EducationResponse
{
    public Guid Id { get; init; }
    public string Degree { get; init; } = string.Empty;
    public string Institution { get; init; } = string.Empty;
    public int StartYear { get; init; }
    public int? EndYear { get; init; }
    public string? Score { get; init; }
}

public record ProjectResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> TechStack { get; init; } = new();
    public string? ProjectUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CertificationResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string IssuingOrganization { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CredentialUrl { get; init; }
}

public record ResumeResponse
{
    public Guid ResumeFileId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;  // Presigned MinIO URL
    public DateTime UploadedAt { get; init; }
}

/// <summary>
/// Lightweight response for Application Service eligibility checks.
/// Returned by /internal/students/{id}/eligibility.
/// </summary>
public record StudentEligibilityResponse
{
    public Guid UserId { get; init; }
    public Guid CollegeId { get; init; }
    public string Branch { get; init; } = string.Empty;
    public int BatchYear { get; init; }
    public double Cgpa { get; init; }
    public bool HasActiveResume { get; init; }
    public int ProfileCompletionScore { get; init; }
}

/// <summary>
/// Quick summary for Recruiter viewing applicant list.
/// Returned by /internal/students/{id}/summary.
/// </summary>
public record StudentSummaryResponse
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public string Branch { get; init; } = string.Empty;
    public double Cgpa { get; init; }
    public int BatchYear { get; init; }
    public int ProfileCompletionScore { get; init; }
}

/// <summary>
/// Used in TPO's paginated student list.
/// </summary>
public record StudentListItemResponse
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Branch { get; init; } = string.Empty;
    public double Cgpa { get; init; }
    public int BatchYear { get; init; }
    public int ProfileCompletionScore { get; init; }
    public bool HasActiveResume { get; init; }
}