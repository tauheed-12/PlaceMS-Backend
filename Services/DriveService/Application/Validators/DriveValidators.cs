using DriveService.Application.DTOs.Requests;
using FluentValidation;
using SharedKernel.Enums;

namespace DriveService.Application.Validators;

public class CreateDriveRequestValidator : AbstractValidator<CreateDriveRequest>
{
    public CreateDriveRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.JobRole)
            .NotEmpty().WithMessage("Job role is required.")
            .MaximumLength(150);

        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage("Job description is required.")
            .MaximumLength(2000);

        RuleFor(x => x.CTC)
            .NotEmpty().WithMessage("CTC is required.")
            .MaximumLength(100);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200);

        RuleFor(x => x.EmploymentType)
            .IsInEnum().WithMessage("Employment type is required.");

        RuleFor(x => x.DriveDate)
            .GreaterThan(DateTime.MinValue).WithMessage("Drive date must be provided.");

        RuleFor(x => x.ApplicationDeadline)
            .GreaterThan(x => x.DriveDate)
            .WithMessage("Application deadline must be after the drive date.");

        RuleFor(x => x.MinCgpa)
            .InclusiveBetween(0.0, 10.0).WithMessage("Min CGPA must be between 0.0 and 10.0.");

        RuleFor(x => x.EligibleBranches)
            .NotEqual(EligibleBranch.None).WithMessage("At least one eligible branch must be selected.");

        RuleFor(x => x.EligibleBatch)
            .GreaterThan(0).WithMessage("Eligible batch is required.");

        RuleFor(x => x.Rounds)
            .NotEmpty().WithMessage("At least one interview round is required.")
            .ForEach(round => round.NotEmpty().WithMessage("Round name must not be empty."));

        RuleFor(x => x.TargetCollegeIds)
            .NotEmpty().WithMessage("At least one target college must be selected.");
    }
}

public class UpdateDriveRequestValidator : AbstractValidator<UpdateDriveRequest>
{
    public UpdateDriveRequestValidator()
    {
        RuleFor(x => x.JobRole)
            .NotEmpty().WithMessage("Job role is required.")
            .MaximumLength(150);

        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage("Job description is required.")
            .MaximumLength(2000);

        RuleFor(x => x.CTC)
            .NotEmpty().WithMessage("CTC is required.")
            .MaximumLength(100);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200);

        RuleFor(x => x.EmploymentType)
            .IsInEnum().WithMessage("Employment type is required.");

        RuleFor(x => x.DriveDate)
            .GreaterThan(DateTime.MinValue).WithMessage("Drive date must be provided.");

        RuleFor(x => x.ApplicationDeadline)
            .GreaterThan(x => x.DriveDate)
            .WithMessage("Application deadline must be after the drive date.");

        RuleFor(x => x.MinCgpa)
            .InclusiveBetween(0.0, 10.0).WithMessage("Min CGPA must be between 0.0 and 10.0.");

        RuleFor(x => x.EligibleBranches)
            .NotEqual(EligibleBranch.None).WithMessage("At least one eligible branch must be selected.");

        RuleFor(x => x.EligibleBatch)
            .GreaterThan(0).WithMessage("Eligible batch is required.");

        RuleFor(x => x.Rounds)
            .NotEmpty().WithMessage("At least one interview round is required.")
            .ForEach(round => round.NotEmpty().WithMessage("Round name must not be empty."));
    }
}

public class ApproveDriveRequestValidator : AbstractValidator<ApproveDriveRequest>
{
    public ApproveDriveRequestValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(1000);
    }
}

public class RejectDriveRequestValidator : AbstractValidator<RejectDriveRequest>
{
    public RejectDriveRequestValidator()
    {
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Rejection note is required.")
            .MaximumLength(1000);
    }
}

public class RequestChangesRequestValidator : AbstractValidator<RequestChangesRequest>
{
    public RequestChangesRequestValidator()
    {
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Change request note is required.")
            .MaximumLength(1000);
    }
}

public class DriveListQueryValidator : AbstractValidator<DriveListQuery>
{
    public DriveListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be at most 100.");
    }
}

public class AvailableDrivesQueryValidator : AbstractValidator<AvailableDrivesQuery>
{
    public AvailableDrivesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be at most 100.");
    }
}

public class AdminDriveListQueryValidator : AbstractValidator<AdminDriveListQuery>
{
    public AdminDriveListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be at most 100.");
    }
}
