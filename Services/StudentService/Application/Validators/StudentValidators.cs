using FluentValidation;
using SharedKernel.Constants;
using StudentService.Application.DTOs.Requests;

namespace StudentService.Application.Validators;

public class UpdatePersonalInfoValidator : AbstractValidator<UpdatePersonalInfoRequest>
{
    public UpdatePersonalInfoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(ValidationRules.PhonePattern)
            .WithMessage("Enter a valid 10-digit Indian mobile number.");

        RuleFor(x => x.Branch)
            .NotEmpty().WithMessage("Branch is required.")
            .MaximumLength(100);

        RuleFor(x => x.BatchYear)
            .InclusiveBetween(2000, DateTime.UtcNow.Year + 5)
            .WithMessage($"Batch year must be between 2000 and {DateTime.UtcNow.Year + 5}.");

        RuleFor(x => x.Cgpa)
            .InclusiveBetween(0.0, 10.0)
            .WithMessage("CGPA must be between 0 and 10.");

        RuleFor(x => x.AboutMe)
            .MaximumLength(1000)
            .WithMessage("About Me must not exceed 1000 characters.")
            .When(x => x.AboutMe is not null);
    }
}

public class AddEducationValidator : AbstractValidator<AddEducationRequest>
{
    public AddEducationValidator()
    {
        RuleFor(x => x.Degree)
            .NotEmpty().WithMessage("Degree is required.")
            .MaximumLength(200);

        RuleFor(x => x.Institution)
            .NotEmpty().WithMessage("Institution is required.")
            .MaximumLength(200);

        RuleFor(x => x.StartYear)
            .InclusiveBetween(1980, DateTime.UtcNow.Year)
            .WithMessage("Start year is invalid.");

        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .WithMessage("End year must be after start year.")
            .When(x => x.EndYear.HasValue);

        RuleFor(x => x.Score)
            .MaximumLength(20)
            .When(x => x.Score is not null);
    }
}

public class UpdateEducationValidator : AbstractValidator<UpdateEducationRequest>
{
    public UpdateEducationValidator()
    {
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartYear).InclusiveBetween(1980, DateTime.UtcNow.Year);
        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .When(x => x.EndYear.HasValue);
    }
}

public class ReplaceSkillsValidator : AbstractValidator<ReplaceSkillsRequest>
{
    public ReplaceSkillsValidator()
    {
        RuleFor(x => x.Skills)
            .NotNull().WithMessage("Skills list is required.")
            .Must(s => s.Count <= 50).WithMessage("Maximum 50 skills allowed.");

        RuleForEach(x => x.Skills)
            .NotEmpty().WithMessage("Skill name cannot be empty.")
            .MaximumLength(50).WithMessage("Each skill must not exceed 50 characters.");
    }
}

public class AddProjectValidator : AbstractValidator<AddProjectRequest>
{
    public AddProjectValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.")
            .MaximumLength(2000);

        RuleFor(x => x.TechStack)
            .NotNull()
            .Must(t => t.Count <= 20).WithMessage("Maximum 20 technologies per project.");

        RuleFor(x => x.ProjectUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Project URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ProjectUrl));
    }
}

public class UpdateProjectValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TechStack).NotNull().Must(t => t.Count <= 20);
        RuleFor(x => x.ProjectUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ProjectUrl));
    }
}

public class AddCertificationValidator : AbstractValidator<AddCertificationRequest>
{
    public AddCertificationValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Certification title is required.")
            .MaximumLength(200);

        RuleFor(x => x.IssuingOrganization)
            .NotEmpty().WithMessage("Issuing organization is required.")
            .MaximumLength(200);

        RuleFor(x => x.IssueDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Issue date cannot be in the future.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.IssueDate)
            .WithMessage("Expiry date must be after issue date.")
            .When(x => x.ExpiryDate.HasValue);

        RuleFor(x => x.CredentialUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Credential URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.CredentialUrl));
    }
}

public class UpdateCertificationValidator : AbstractValidator<UpdateCertificationRequest>
{
    public UpdateCertificationValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssuingOrganization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssueDate).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.IssueDate)
            .When(x => x.ExpiryDate.HasValue);
    }
}