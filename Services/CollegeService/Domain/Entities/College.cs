using CollegeService.Domain.Events;
using SharedKernel.Abstractions;
using SharedKernel.Enums;
using SharedKernel.Exceptions;

namespace CollegeService.Domain.Entities;

public class College : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;
    public string AffiliatedBy { get; private set; } = string.Empty;
    public CollegeType Type { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Pincode { get; private set; } = string.Empty;
    public string RegisteredBy { get; private set; } = string.Empty;
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Unverified;

    // Email Verification
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    public College() { }

    public static College Create(string name, string code, string email, string phone, string website, string affiliatedBy, CollegeType type, string city, string state, string pincode, string createdBy)
    {
        var college = new College
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            Website = website.Trim().ToLowerInvariant(),
            AffiliatedBy = affiliatedBy.Trim(),
            Type = type,
            City = city.Trim(),
            State = state.Trim(),
            Pincode = pincode.Trim(),
            RegisteredBy = createdBy,
            VerificationStatus = VerificationStatus.Unverified
        };
        college.RaiseDomainEvent(new CollegeRegisterDomainEvent(college.Id, college.Email, college.Name, college.Code, college.RegisteredBy));
        college.SetUpdatedAt();

        return college;
    }

    public string GenerateEmailVerificationToken()
    {
        var token = GenerateSecureToken();
        EmailVerificationToken = token;
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        SetUpdatedAt();
        return token;
    }

    public void VerifyEmail(string token)
    {
        if (VerificationStatus == VerificationStatus.Verified)
            throw new InvalidOperationDomainException("Email is already verified.");

        if (VerificationStatus == VerificationStatus.Deactivated)
            throw new InvalidOperationDomainException("Account is deactivated.");

        if (EmailVerificationToken is null || EmailVerificationTokenExpiry is null)
            throw new DomainValidationException("No verification token found. Request a new one.");

        if (EmailVerificationToken != token)
            throw new DomainValidationException("Invalid verification token.");

        if (DateTime.UtcNow > EmailVerificationTokenExpiry)
            throw new DomainValidationException("Verification token has expired. Request a new one.");

        VerificationStatus = VerificationStatus.Verified;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
        SetUpdatedAt();

        // RaiseDomainEvent(new UserEmailVerifiedDomainEvent(Id, Email));
    }

    public void UpdateDetails(string name, string code, string email, string phone, string website, string affiliatedBy, CollegeType type, string city, string state, string pincode, string updatedBy)
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Website = website.Trim().ToLowerInvariant();
        AffiliatedBy = affiliatedBy.Trim();
        Type = type;
        City = city.Trim();
        State = state.Trim();
        Pincode = pincode.Trim();
        RegisteredBy = updatedBy;
        SetUpdatedAt();
    }

    // ------------ Status Management --------------------
    public void Deactivate()
    {
        if (VerificationStatus == VerificationStatus.Deactivated)
            throw new InvalidOperationDomainException("College is already deactivated");

        VerificationStatus = VerificationStatus.Deactivated;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        if (VerificationStatus != VerificationStatus.Deactivated)
            throw new InvalidOperationDomainException("College is not Deactivated");

        VerificationStatus = VerificationStatus.Verified;
        SetUpdatedAt();
    }

    // ------ Generate secure token --------------
    public static string GenerateSecureToken()
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}