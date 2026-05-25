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
    public AccountStatus AccountStatus { get; private set; } = AccountStatus.Active;

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
        };
        college.RaiseDomainEvent(new CollegeRegisterDomainEvent(college.Id, college.Email, college.Name, college.Code, college.RegisteredBy));
        college.SetUpdatedAt();

        return college;
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
        if (AccountStatus == AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("College is already deactivated");

        AccountStatus = AccountStatus.Deactivated;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        if (AccountStatus != AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("College is not Deactivated");

        AccountStatus = AccountStatus.Active;
        SetUpdatedAt();
    }

    // ------ Generate secure token --------------
    public static string GenerateSecureToken()
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}