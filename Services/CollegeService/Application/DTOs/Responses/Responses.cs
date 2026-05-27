using SharedKernel.Enums;

namespace CollegeService.Application.DTOs.Responses;

public record CollegeDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public string AffiliatedBy { get; init; } = string.Empty;
    public CollegeType Type { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Pincode { get; init; } = string.Empty;
    public AccountStatus AccountStatus { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool HasTpoAssigned { get; init; }
}

public record CreateCollegeResponseDto
{
    public CollegeShortDto College { get; init; } = null!;
    public string Message { get; init; } = "College registered successfully";
}

public record UpdateCollegeResponseDto
{
    public CollegeShortDto College { get; init; } = null!;
    public string Message { get; init; } = "College updated successfully";
}

public record CollegeShortDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public AccountStatus AccountStatus { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool HasTpoAssigned { get; init; }
}

public class TpoDetailsDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid CollegeId { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string CollegeCode { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedResponseDto<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public class ValidateCollegeCodeResponseDto
{
    public bool IsValid { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public class AdminCollegeScopeResponseDto
{
    public Guid AdminId { get; init; }
    public Guid CollegeId { get; init; }
    public string CollegeName { get; init; } = string.Empty;
    public string CollegeCode { get; init; } = string.Empty;
}