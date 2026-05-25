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
    public Guid RegisteredBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public AccountStatus AccountStatus { get; private set; } = AccountStatus.Active;

    public College() { }

    public static College Create(string name, string code, string email, string phone, string website, string affiliatedBy, CollegeType type, string city, string state, string pincode, Guid createdBy)
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
        college.RaiseDomainEvent(new CollegeCreatedDomainEvent(college.Id, college.Name, college.Code, createdBy));
        college.SetUpdatedAt();

        return college;
    }

    public void UpdateDetails(string name, string code, string email, string phone, string website, string affiliatedBy, CollegeType type, string city, string state, string pincode, Guid updatedBy)
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
        UpdatedBy = updatedBy;
        SetUpdatedAt();
    }

    // ------------ Status Management --------------------
    public void Deactivate(Guid deactivatedBy)
    {
        if (AccountStatus == AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("College is already deactivated");

        AccountStatus = AccountStatus.Deactivated;
        UpdatedBy = deactivatedBy;
        SetUpdatedAt();

        RaiseDomainEvent(new CollegeDeactivatedDomainEvent(Id, Name, Code, deactivatedBy));
    }

    public void Reactivate(Guid activatedBy)
    {
        if (AccountStatus != AccountStatus.Deactivated)
            throw new InvalidOperationDomainException("College is not Deactivated");

        AccountStatus = AccountStatus.Active;
        UpdatedBy = activatedBy;
        SetUpdatedAt();
        RaiseDomainEvent(new CollegeActivatedDomainEvent(Id, Name, Code, activatedBy));
    }
}