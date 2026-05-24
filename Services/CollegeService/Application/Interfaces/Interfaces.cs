using System.ComponentModel.DataAnnotations;
using CollegeService.Application.DTOs.Requests;
using CollegeService.Application.DTOs.Responses;
using CollegeService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Abstractions;
using SharedKernel.Enums;
using SharedKernel.Models;

namespace CollegeService.Application.Interfaces;

public interface ICollegeService
{
    Task<CreateCollegeResponseDto> RegisterAsync(CreateCollegeRequestDto request, string registeredBy, CancellationToken ct);
    // Task VerifyCollegeEmailAsync(string token, CancellationToken ct);
    Task<UpdateCollegeResponseDto> UpdateAsync(UpdateCollegeRequestDto request, string updatedBy, CancellationToken ct);
    Task DeactivateCollegeAsync(Guid collegeId, CancellationToken ct);
    Task ReactivateCollegeAsync(Guid collegeId, CancellationToken ct);
}

public interface ICollegeQueryService
{
    Task<PaginatedResponseDto<CollegeShortDto>> GetAllCollegesAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<CollegeDetailsDto> GetCollegeByIdAsync(Guid id, CancellationToken ct);
    Task<CollegeDetailsDto> GetCollegeByCodeAsync(string code, CancellationToken ct);
    Task<List<CollegeShortDto>> GetCollegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<List<CollegeShortDto>> GetCollegesByCodesAsync(IEnumerable<string> codes, CancellationToken ct);
    Task<PagedResult<CollegeShortDto>> GetFilteredCollegesAsync(CollegeFilterRequestDto filter, CancellationToken ct = default);

    Task<PaginatedResponseDto<CollegeShortDto>> GetCollegesByAdminIdAsync(
        Guid adminId,
        int pageNumber,
        int pageSize,
        CancellationToken ct);

    Task<ValidateCollegeCodeResponseDto> ValidateCollegeAsync(string code, CancellationToken ct);
}

public interface ICollegeRepository
{
    Task<College?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<College?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<College?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<List<College>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<List<College>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);

    Task<(IEnumerable<College> Items, int TotalCount)> GetFilteredAsync(CollegeFilterRequestDto filter, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task AddAsync(College college, CancellationToken ct = default);
    void Update(College college);
    public IQueryable<College> GetQueryable();

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IAdminCollegeScopeService
{
    Task<AdminCollegeScopeResponseDto> AssignCollegeToAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task RemoveCollegeFromAdminAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task<bool> HasAccessToCollegeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct);
    Task<List<Guid>> GetCollegesByAdminIdAsync(Guid adminUserId, CancellationToken ct);
    Task<List<TpoDetailsDto>> GetTposByAdminIdAsync(Guid adminUserId, CancellationToken ct);
}

public interface IAdminCollegeScopeRepository
{
    Task<List<Guid>> GetCollegeIdsByAdminIdAsync(Guid adminId, CancellationToken ct = default);
    Task AddScopeAsync(AdminCollegeScope scope, CancellationToken ct = default);
    Task<AdminCollegeScope?> GetScopeAsync(Guid adminUserId, Guid collegeId, CancellationToken ct = default);
    void RemoveScope(AdminCollegeScope scope);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface ICollegeTpoService
{
    Task<TpoDetailsDto> AssignPrimaryTpoAsync(Guid collegeId, CreateTpoRequestDto request, Guid assignedBy, CancellationToken ct);
    Task RemoveTpoAsync(Guid collegeId, Guid userId, CancellationToken ct);
    // Task<List<TpoShortDto>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct);
    Task<TpoDetailsDto?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct);
    Task<bool> IsPrimaryTpoAsync(Guid collegeId, Guid userId, CancellationToken ct);
}

public interface ICollegeTpoRepository
{
    Task<CollegeTpo?> GetPrimaryTpoByCollegeIdAsync(Guid collegeId, CancellationToken ct = default);
    Task<CollegeTpo?> GetTpoByEmailAsync(string email, CancellationToken ct = default);
    Task<List<CollegeTpo>> GetTposByCollegeIdAsync(Guid collegeId, CancellationToken ct = default);
    Task<List<Guid>> GetCollegeIdsHavingPrimaryTpoAsync(List<Guid> collegeIds, CancellationToken ct);
    Task<(IEnumerable<College> Items, int TotalCount)> GetFilteredAsync(CollegeFilterRequestDto filter, CancellationToken ct = default);
    Task AddAsync(CollegeTpo collegeTpo, CancellationToken ct = default);
    void Update(CollegeTpo collegeTpo);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IIdentityServiceClient
{
    Task<TpoRegistrationResult?> RegisterTpoAsync(RegisterTpoIdentityRequestDto request, CancellationToken ct);
    Task<TpoDetails?> GetTpoDetails(Guid tpoId, CancellationToken ct);
    Task<List<TpoDetails>?> GetTpoDetailsByIdsBatchAsync(List<Guid> tpoIds, CancellationToken ct);
}

public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}

public record TpoRegistrationResult
{
    public Guid UserId { get; init; }
}

public record TpoDetails
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public VerificationStatus VerificationStatus { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid CollegeId { get; init; }
    public string CollegeCode { get; init; } = string.Empty;
    public string CollegeName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}