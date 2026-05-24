using SharedKernel.Enums;

namespace CollegeService.Application.DTOs.Requests;

public record CreateCollegeRequestDto
{
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
}

public record UpdateCollegeRequestDto
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
}

public record CollegeFilterRequestDto
{
    public string? Search { get; init; }
    public string? State { get; init; }
    public string? City { get; init; }
    public VerificationStatus? VerificationStatus { get; init; }
    public bool? HasTpoAssigned { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record CreateTpoRequestDto
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}


public class RegisterTpoIdentityRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public Guid CollegeId { get; set; }

    public string CollegeCode { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.TPO;
}