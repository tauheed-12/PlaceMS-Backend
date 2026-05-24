using FluentValidation;
using CollegeService.Application.DTOs.Requests;
using SharedKernel.Constants;

namespace CollegeService.Application.Validators;

public class CreateCollegeRequestValidator : AbstractValidator<CreateCollegeRequestDto>
{
    public CreateCollegeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("College name is required.")
            .MinimumLength(ValidationRules.NameMinLength)
                .WithMessage($"College name must be at least {ValidationRules.NameMinLength} characters.")
            .MaximumLength(ValidationRules.CollegeNameMaxLength)
                .WithMessage($"College name must not exceed {ValidationRules.CollegeNameMaxLength} characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("College code is required.")
            .Matches(ValidationRules.CollegeCodePattern)
                .WithMessage("College code must be uppercase alphanumeric (e.g. IITLKO).")
            .MinimumLength(ValidationRules.CollegeCodeMinLength)
                .WithMessage($"College code must be at least {ValidationRules.CollegeCodeMinLength} characters.")
            .MaximumLength(ValidationRules.CollegeCodeMaxLength)
                .WithMessage($"College code must not exceed {ValidationRules.CollegeCodeMaxLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("College email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(ValidationRules.EmailMaxLength);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(ValidationRules.PhonePattern)
                .WithMessage("Enter a valid 10-digit Indian mobile number.");

        RuleFor(x => x.Website)
            .NotEmpty().WithMessage("Website URL is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var result)
                         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            .WithMessage("A valid website URL is required (e.g. https://college.ac.in).");

        RuleFor(x => x.AffiliatedBy)
            .NotEmpty().WithMessage("Affiliation is required.")
            .MaximumLength(ValidationRules.CollegeAffiliatedByMaxLength)
                .WithMessage($"Affiliated by must not exceed {ValidationRules.CollegeAffiliatedByMaxLength} characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid college type specified.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(ValidationRules.CollegeCityMaxLength)
                .WithMessage($"City must not exceed {ValidationRules.CollegeCityMaxLength} characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(ValidationRules.CollegeStateMaxLength)
                .WithMessage($"State must not exceed {ValidationRules.CollegeStateMaxLength} characters.");

        RuleFor(x => x.Pincode)
            .NotEmpty().WithMessage("Pincode is required.")
            .Matches(@"^\d{6}$").WithMessage("Enter a valid 6-digit Indian pincode.");
    }
}

public class UpdateCollegeRequestValidator : AbstractValidator<UpdateCollegeRequestDto>
{
    public UpdateCollegeRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("A valid College ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("College name is required.")
            .MinimumLength(ValidationRules.NameMinLength)
                .WithMessage($"College name must be at least {ValidationRules.NameMinLength} characters.")
            .MaximumLength(ValidationRules.CollegeNameMaxLength)
                .WithMessage($"College name must not exceed {ValidationRules.CollegeNameMaxLength} characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("College code is required.")
            .Matches(ValidationRules.CollegeCodePattern)
                .WithMessage("College code must be uppercase alphanumeric (e.g. IITLKO).")
            .MinimumLength(ValidationRules.CollegeCodeMinLength)
                .WithMessage($"College code must be at least {ValidationRules.CollegeCodeMinLength} characters.")
            .MaximumLength(ValidationRules.CollegeCodeMaxLength)
                .WithMessage($"College code must not exceed {ValidationRules.CollegeCodeMaxLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("College email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(ValidationRules.EmailMaxLength);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(ValidationRules.PhonePattern)
                .WithMessage("Enter a valid 10-digit Indian mobile number.");

        RuleFor(x => x.Website)
            .NotEmpty().WithMessage("Website URL is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var result)
                         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            .WithMessage("A valid website URL is required (e.g. https://college.ac.in).");

        RuleFor(x => x.AffiliatedBy)
            .NotEmpty().WithMessage("Affiliation is required.")
            .MaximumLength(ValidationRules.CollegeAffiliatedByMaxLength)
                .WithMessage($"Affiliated by must not exceed {ValidationRules.CollegeAffiliatedByMaxLength} characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid college type specified.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(ValidationRules.CollegeCityMaxLength)
                .WithMessage($"City must not exceed {ValidationRules.CollegeCityMaxLength} characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(ValidationRules.CollegeStateMaxLength)
                .WithMessage($"State must not exceed {ValidationRules.CollegeStateMaxLength} characters.");

        RuleFor(x => x.Pincode)
            .NotEmpty().WithMessage("Pincode is required.")
            .Matches(@"^\d{6}$").WithMessage("Enter a valid 6-digit Indian pincode.");
    }
}

public class CollegeFilterRequestValidator : AbstractValidator<CollegeFilterRequestDto>
{
    private static readonly int[] AllowedPageSizes = [10, 25, 50, 100];

    public CollegeFilterRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .Must(size => AllowedPageSizes.Contains(size))
            .WithMessage($"Page size must be one of: {string.Join(", ", AllowedPageSizes)}.");

        RuleFor(x => x.VerificationStatus)
            .IsInEnum().WithMessage("Invalid verification status.")
            .When(x => x.VerificationStatus.HasValue);
    }
}

public class CreateTpoRequestValidator : AbstractValidator<CreateTpoRequestDto>
{
    public CreateTpoRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(ValidationRules.NameMinLength)
            .MaximumLength(ValidationRules.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(ValidationRules.EmailMaxLength);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(ValidationRules.PhonePattern)
                .WithMessage("Enter a valid 10-digit Indian mobile number.");
    }
}