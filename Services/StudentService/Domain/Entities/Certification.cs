using SharedKernel.Abstractions;
using SharedKernel.Exceptions;
using StudentService.Domain.Events;

public class Certification : BaseEntity
{
    public Guid StudentProfileId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string IssuingOrganization { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string? CredentialUrl { get; private set; }

    private Certification() { }

    public static Certification Create(
        Guid studentProfileId,
        string title,
        string issuingOrganization,
        DateTime issueDate,
        DateTime? expiryDate,
        string? credentialUrl)
    {
        if (expiryDate.HasValue && expiryDate < issueDate)
            throw new DomainValidationException("Expiry date cannot be before issue date.");

        return new Certification
        {
            StudentProfileId = studentProfileId,
            Title = title.Trim(),
            IssuingOrganization = issuingOrganization.Trim(),
            IssueDate = issueDate.ToUniversalTime(),
            ExpiryDate = expiryDate?.ToUniversalTime(),
            CredentialUrl = credentialUrl?.Trim()
        };
    }

    public void Update(string title, string issuingOrganization,
        DateTime issueDate, DateTime? expiryDate, string? credentialUrl)
    {
        if (expiryDate.HasValue && expiryDate < issueDate)
            throw new DomainValidationException("Expiry date cannot be before issue date.");

        Title = title.Trim();
        IssuingOrganization = issuingOrganization.Trim();
        IssueDate = issueDate.ToUniversalTime();
        ExpiryDate = expiryDate?.ToUniversalTime();
        CredentialUrl = credentialUrl?.Trim();
        SetUpdatedAt();
    }
}