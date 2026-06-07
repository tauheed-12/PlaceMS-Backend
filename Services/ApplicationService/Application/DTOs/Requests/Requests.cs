using SharedKernel.Enums;

namespace ApplicationService.Application.DTOs.Requests;

public record CreateApplicationRequestDto
{
    public Guid DriveId { get; init; }
}

public record UpdateApplicationStatusRequestDto
{
    public ApplicationStatus Status { get; init; }
}

public record ApplicationFilterRequestDto
{
    public string? Search { get; init; }
    public ApplicationStatus? Status { get; init; }
    public Guid? CollegeId { get; init; }
    public DateTime? AppliedFrom { get; init; }
    public DateTime? AppliedTo { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}