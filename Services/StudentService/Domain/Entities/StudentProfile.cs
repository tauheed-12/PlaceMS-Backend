using SharedKernel.Abstractions;
using SharedKernel.Exceptions;
using StudentService.Domain.Events;

namespace StudentService.Domain.Entities;

/// <summary>
/// StudentProfile aggregate root.
/// Owns all profile sections. Business rules enforced here.
/// ProfileCompletionScore is recalculated and stored on every save.
/// </summary>
public class StudentProfile : AggregateRoot
{
    // ── Identity (mirrors Identity Service — no FK, just Guid) ────
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;

    // ── Academic Info ─────────────────────────────────────────────
    public Guid CollegeId { get; private set; }
    public string CollegeCode { get; private set; } = string.Empty;
    public string CollegeName { get; private set; } = string.Empty;
    public string Branch { get; private set; } = string.Empty;
    public int BatchYear { get; private set; }
    public double Cgpa { get; private set; }

    // ── About ─────────────────────────────────────────────────────
    public string? AboutMe { get; private set; }

    // ── Profile Completion ────────────────────────────────────────
    public int ProfileCompletionScore { get; private set; } = 0;

    // ── Resume ────────────────────────────────────────────────────
    public Guid? ActiveResumeFileId { get; private set; }

    // ── Navigation Collections ────────────────────────────────────
    private readonly List<Education> _education = new();
    private readonly List<Skill> _skills = new();
    private readonly List<Project> _projects = new();
    private readonly List<Certification> _certifications = new();
    private readonly List<ResumeFile> _resumeFiles = new();

    public IReadOnlyCollection<Education> Educations => _education.AsReadOnly();
    public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();
    public IReadOnlyCollection<Certification> Certifications => _certifications.AsReadOnly();
    public IReadOnlyCollection<ResumeFile> ResumeFiles => _resumeFiles.AsReadOnly();

    // EF Core constructor
    private StudentProfile() { }

    // ── Factory ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a skeleton profile when a student registers.
    /// Called by the Kafka consumer on pms.user.registered event.
    /// Completion starts at 0 — student fills sections progressively.
    /// </summary>
    public static StudentProfile CreateSkeleton(
        Guid userId,
        string fullName,
        string email,
        string phoneNumber,
        Guid collegeId,
        string collegeCode,
        string collegeName)
    {
        var profile = new StudentProfile
        {
            UserId = userId,
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            CollegeId = collegeId,
            CollegeCode = collegeCode.ToUpperInvariant(),
            CollegeName = collegeName
        };

        profile.RecalculateCompletionScore();
        profile.RaiseDomainEvent(new StudentProfileCreatedDomainEvent(userId, email, collegeId));
        return profile;
    }

    // ── Personal Info Update ──────────────────────────────────────

    public void UpdatePersonalInfo(
        string fullName,
        string phoneNumber,
        string branch,
        int batchYear,
        double cgpa,
        string? aboutMe)
    {
        if (cgpa is < 0 or > 10)
            throw new DomainValidationException("CGPA must be between 0 and 10.");

        if (batchYear < 2000 || batchYear > DateTime.UtcNow.Year + 5)
            throw new DomainValidationException($"Batch year must be between 2000 and {DateTime.UtcNow.Year + 5}.");

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        Branch = branch.Trim();
        BatchYear = batchYear;
        Cgpa = cgpa;
        AboutMe = aboutMe?.Trim();

        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    // ── Education ─────────────────────────────────────────────────

    public Education AddEducation(
        string degree,
        string institution,
        int startYear,
        int? endYear,
        string? score)
    {
        var entry = Education.Create(Id, degree, institution, startYear, endYear, score);
        _education.Add(entry);
        RecalculateCompletionScore();
        SetUpdatedAt();
        return entry;
    }

    public void UpdateEducation(Guid educationId, string degree, string institution,
        int startYear, int? endYear, string? score)
    {
        var entry = _education.FirstOrDefault(e => e.Id == educationId)
            ?? throw new NotFoundException("Education", educationId);

        entry.Update(degree, institution, startYear, endYear, score);
        SetUpdatedAt();
    }

    public void RemoveEducation(Guid educationId)
    {
        var entry = _education.FirstOrDefault(e => e.Id == educationId)
            ?? throw new NotFoundException("Education", educationId);

        _education.Remove(entry);
        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    // ── Skills ────────────────────────────────────────────────────

    /// <summary>
    /// Replaces the entire skill list atomically.
    /// Deduplicates by name (case-insensitive).
    /// </summary>
    public void ReplaceSkills(IEnumerable<string> skillNames)
    {
        var distinct = skillNames
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .DistinctBy(s => s.ToLowerInvariant())
            .ToList();

        if (distinct.Count > 50)
            throw new DomainValidationException("Maximum 50 skills allowed.");

        _skills.Clear();
        foreach (var name in distinct)
            _skills.Add(Skill.Create(Id, name));

        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    // ── Projects ─────────────────────────────────────────────────

    public Project AddProject(
        string title,
        string description,
        List<string> techStack,
        string? projectUrl)
    {
        if (_projects.Count >= 10)
            throw new DomainValidationException("Maximum 10 projects allowed.");

        var project = Project.Create(Id, title, description, techStack, projectUrl);
        _projects.Add(project);
        RecalculateCompletionScore();
        SetUpdatedAt();
        return project;
    }

    public void UpdateProject(Guid projectId, string title, string description,
        List<string> techStack, string? projectUrl)
    {
        var project = _projects.FirstOrDefault(p => p.Id == projectId)
            ?? throw new NotFoundException("Project", projectId);

        project.Update(title, description, techStack, projectUrl);
        SetUpdatedAt();
    }

    public void RemoveProject(Guid projectId)
    {
        var project = _projects.FirstOrDefault(p => p.Id == projectId)
            ?? throw new NotFoundException("Project", projectId);

        _projects.Remove(project);
        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    // ── Certifications ────────────────────────────────────────────

    public Certification AddCertification(
        string title,
        string issuingOrganization,
        DateTime issueDate,
        DateTime? expiryDate,
        string? credentialUrl)
    {
        if (_certifications.Count >= 20)
            throw new DomainValidationException("Maximum 20 certifications allowed.");

        var cert = Certification.Create(Id, title, issuingOrganization, issueDate, expiryDate, credentialUrl);
        _certifications.Add(cert);
        RecalculateCompletionScore();
        SetUpdatedAt();
        return cert;
    }

    public void UpdateCertification(Guid certId, string title, string issuingOrganization,
        DateTime issueDate, DateTime? expiryDate, string? credentialUrl)
    {
        var cert = _certifications.FirstOrDefault(c => c.Id == certId)
            ?? throw new NotFoundException("Certification", certId);

        cert.Update(title, issuingOrganization, issueDate, expiryDate, credentialUrl);
        SetUpdatedAt();
    }

    public void RemoveCertification(Guid certId)
    {
        var cert = _certifications.FirstOrDefault(c => c.Id == certId)
            ?? throw new NotFoundException("Certification", certId);

        _certifications.Remove(cert);
        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    // ── Resume ────────────────────────────────────────────────────

    /// <summary>
    /// Sets a new uploaded resume as active.
    /// Marks all previous resumes as inactive.
    /// </summary>
    public void SetActiveResume(ResumeFile resumeFile)
    {
        // Deactivate all existing resumes
        foreach (var r in _resumeFiles.Where(r => r.IsActive))
            r.Deactivate();

        resumeFile.Activate();
        _resumeFiles.Add(resumeFile);
        ActiveResumeFileId = resumeFile.Id;

        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    public void RemoveResume()
    {
        var active = _resumeFiles.FirstOrDefault(r => r.IsActive);
        if (active is null)
            throw new NotFoundException("No active resume found.");

        active.Deactivate();
        ActiveResumeFileId = null;

        RecalculateCompletionScore();
        SetUpdatedAt();
    }

    // ── Profile Completion Calculator ─────────────────────────────

    /// <summary>
    /// Recalculates and stores the profile completion score.
    /// Called after every mutation — never called externally.
    /// 
    /// Weights:
    ///   Personal Info  20%  (name, phone, branch, batch, cgpa all filled)
    ///   Education      20%  (at least 1 entry)
    ///   Skills         15%  (at least 3 skills)
    ///   Projects       20%  (at least 1 project)
    ///   Certifications 10%  (at least 1 certification)
    ///   Resume         15%  (active resume uploaded)
    /// </summary>
    private void RecalculateCompletionScore()
    {
        var score = 0;

        // Personal Info (20%)
        if (!string.IsNullOrWhiteSpace(FullName)
            && !string.IsNullOrWhiteSpace(PhoneNumber)
            && !string.IsNullOrWhiteSpace(Branch)
            && BatchYear > 0
            && Cgpa > 0)
            score += 20;

        // Education (20%)
        if (_education.Any())
            score += 20;

        // Skills (15%) — at least 3
        if (_skills.Count >= 3)
            score += 15;

        // Projects (20%) — at least 1
        if (_projects.Any())
            score += 20;

        // Certifications (10%) — at least 1
        if (_certifications.Any())
            score += 10;

        // Resume (15%) — active resume uploaded
        if (ActiveResumeFileId.HasValue)
            score += 15;

        ProfileCompletionScore = score;
    }
}