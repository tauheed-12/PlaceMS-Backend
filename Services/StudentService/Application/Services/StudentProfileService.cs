using SharedKernel.Exceptions;
using SharedKernel.Models;
using StudentService.Application.DTOs.Requests;
using StudentService.Application.DTOs.Responses;
using StudentService.Application.Interfaces;
using StudentService.Domain.Entities;

namespace StudentService.Application.Services;

public class StudentProfileService : IStudentProfileService
{
    private readonly IStudentRepository _repository;
    private readonly ILogger<StudentProfileService> _logger;

    public StudentProfileService(IStudentRepository repository, ILogger<StudentProfileService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // ── Skeleton Creation (called by Kafka consumer) ───────────────

    public async Task CreateSkeletonAsync(CreateStudentProfileRequest request, CancellationToken ct = default)
    {
        if (await _repository.ExistsByUserIdAsync(request.UserId, ct))
        {
            _logger.LogWarning("Student profile already exists for userId {UserId}. Skipping.", request.UserId);
            return;
        }

        var profile = StudentProfile.CreateSkeleton(
            request.UserId,
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.CollegeId,
            request.CollegeCode,
            request.CollegeName);

        await _repository.AddAsync(profile, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Created skeleton profile for student {UserId}", request.UserId);
    }

    // ── Profile Read ──────────────────────────────────────────────

    public async Task<StudentProfileResponse> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        return MapToResponse(profile);
    }

    public async Task<StudentProfileResponse> GetProfileByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        return MapToResponse(profile);
    }

    // ── Personal Info ─────────────────────────────────────────────

    public async Task<StudentProfileResponse> UpdatePersonalInfoAsync(
        Guid userId,
        UpdatePersonalInfoRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.UpdatePersonalInfo(
            request.FullName,
            request.PhoneNumber,
            request.Branch,
            request.BatchYear,
            request.Cgpa,
            request.AboutMe);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return MapToResponse(profile);
    }

    // ── Education ─────────────────────────────────────────────────

    public async Task<EducationResponse> AddEducationAsync(
        Guid userId,
        AddEducationRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        var entry = profile.AddEducation(
            request.Degree,
            request.Institution,
            request.StartYear,
            request.EndYear,
            request.Score);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return MapEducation(entry);
    }

    public async Task<EducationResponse> UpdateEducationAsync(
        Guid userId,
        Guid educationId,
        UpdateEducationRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.UpdateEducation(educationId, request.Degree, request.Institution,
            request.StartYear, request.EndYear, request.Score);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        var entry = profile.Educations.First(e => e.Id == educationId);
        return MapEducation(entry);
    }

    public async Task DeleteEducationAsync(Guid userId, Guid educationId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.RemoveEducation(educationId);
        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);
    }

    // ── Skills ────────────────────────────────────────────────────

    public async Task<List<string>> ReplaceSkillsAsync(
        Guid userId,
        ReplaceSkillsRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.ReplaceSkills(request.Skills);
        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return profile.Skills.Select(s => s.Name).ToList();
    }

    // ── Projects ─────────────────────────────────────────────────

    public async Task<ProjectResponse> AddProjectAsync(
        Guid userId,
        AddProjectRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        var project = profile.AddProject(
            request.Title,
            request.Description,
            request.TechStack,
            request.ProjectUrl);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return MapProject(project);
    }

    public async Task<ProjectResponse> UpdateProjectAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.UpdateProject(projectId, request.Title, request.Description,
            request.TechStack, request.ProjectUrl);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return MapProject(profile.Projects.First(p => p.Id == projectId));
    }

    public async Task DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.RemoveProject(projectId);
        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);
    }

    // ── Certifications ────────────────────────────────────────────

    public async Task<CertificationResponse> AddCertificationAsync(
        Guid userId,
        AddCertificationRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        var cert = profile.AddCertification(
            request.Title,
            request.IssuingOrganization,
            request.IssueDate,
            request.ExpiryDate,
            request.CredentialUrl);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return MapCertification(cert);
    }

    public async Task<CertificationResponse> UpdateCertificationAsync(
        Guid userId,
        Guid certId,
        UpdateCertificationRequest request,
        CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.UpdateCertification(certId, request.Title, request.IssuingOrganization,
            request.IssueDate, request.ExpiryDate, request.CredentialUrl);

        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);

        return MapCertification(profile.Certifications.First(c => c.Id == certId));
    }

    public async Task DeleteCertificationAsync(Guid userId, Guid certId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        profile.RemoveCertification(certId);
        _repository.Update(profile);
        await _repository.SaveChangesAsync(ct);
    }

    // ── TPO / Internal Queries ────────────────────────────────────

    public async Task<PagedResult<StudentListItemResponse>> GetStudentsByCollegeAsync(
        Guid collegeId,
        int page,
        int pageSize,
        string? search,
        CancellationToken ct = default)
    {
        var paged = await _repository.GetByCollegeIdAsync(collegeId, page, pageSize, search, ct);

        var items = paged.Items.Select(p => new StudentListItemResponse
        {
            UserId = p.UserId,
            FullName = p.FullName,
            Email = p.Email,
            Branch = p.Branch,
            Cgpa = p.Cgpa,
            BatchYear = p.BatchYear,
            ProfileCompletionScore = p.ProfileCompletionScore,
            HasActiveResume = p.ActiveResumeFileId.HasValue
        }).ToList();

        return PagedResult<StudentListItemResponse>.Create(items, paged.TotalCount, page, pageSize);
    }

    public async Task<StudentEligibilityResponse> GetEligibilityAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        return new StudentEligibilityResponse
        {
            UserId = profile.UserId,
            CollegeId = profile.CollegeId,
            Branch = profile.Branch,
            BatchYear = profile.BatchYear,
            Cgpa = profile.Cgpa,
            HasActiveResume = profile.ActiveResumeFileId.HasValue,
            ProfileCompletionScore = profile.ProfileCompletionScore
        };
    }

    public async Task<StudentSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _repository.GetByUserIdWithAllAsync(userId, ct)
            ?? throw new NotFoundException("StudentProfile", userId);

        return new StudentSummaryResponse
        {
            UserId = profile.UserId,
            FullName = profile.FullName,
            Email = profile.Email,
            CollegeName = profile.CollegeName,
            Branch = profile.Branch,
            Cgpa = profile.Cgpa,
            BatchYear = profile.BatchYear,
            ProfileCompletionScore = profile.ProfileCompletionScore
        };
    }

    // ── Mappers ───────────────────────────────────────────────────

    private static StudentProfileResponse MapToResponse(StudentProfile p) => new()
    {
        UserId = p.UserId,
        FullName = p.FullName,
        Email = p.Email,
        PhoneNumber = p.PhoneNumber,
        CollegeName = p.CollegeName,
        CollegeCode = p.CollegeCode,
        CollegeId = p.CollegeId,
        Branch = p.Branch,
        BatchYear = p.BatchYear,
        Cgpa = p.Cgpa,
        AboutMe = p.AboutMe,
        ProfileCompletionScore = p.ProfileCompletionScore,
        HasActiveResume = p.ActiveResumeFileId.HasValue,
        Education = p.Educations.Select(MapEducation).ToList(),
        Skills = p.Skills.Select(s => s.Name).ToList(),
        Projects = p.Projects.Select(MapProject).ToList(),
        Certifications = p.Certifications.Select(MapCertification).ToList(),
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static EducationResponse MapEducation(Education e) => new()
    {
        Id = e.Id,
        Degree = e.Degree,
        Institution = e.Institution,
        StartYear = e.StartYear,
        EndYear = e.EndYear,
        Score = e.Score
    };

    private static ProjectResponse MapProject(Project p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        TechStack = p.TechStack,
        ProjectUrl = p.ProjectUrl,
        CreatedAt = p.CreatedAt
    };

    private static CertificationResponse MapCertification(Certification c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        IssuingOrganization = c.IssuingOrganization,
        IssueDate = c.IssueDate,
        ExpiryDate = c.ExpiryDate,
        CredentialUrl = c.CredentialUrl
    };
}