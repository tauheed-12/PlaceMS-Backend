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

public class CollegeFilterRequestDto
{
    public string? Search { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public AccountStatus? AccountStatus { get; set; }
    public bool? HasTpoAssigned { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class TpoFilterRequestDto
{
    public string? Search { get; set; }
    public bool? IsPrimary { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record CreateTpoRequestDto
{
    public Guid CollegeId { get; init; }
    public string CollegeCode { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
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

public record AdminCollegeScopeRequestDto
{
    public Guid AdminId { get; init; }
    public Guid CollegeId { get; init; }
}