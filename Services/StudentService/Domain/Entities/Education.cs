using SharedKernel.Abstractions;
using SharedKernel.Exceptions;
using StudentService.Domain.Events;

public class Education : BaseEntity
{
    public Guid StudentProfileId { get; private set; }
    public string Degree { get; private set; } = string.Empty;
    public string Institution { get; private set; } = string.Empty;
    public int StartYear { get; private set; }
    public int? EndYear { get; private set; }       // null = currently studying
    public string? Score { get; private set; }      // "8.9 CGPA" or "94.5%"

    private Education() { }

    public static Education Create(
        Guid studentProfileId,
        string degree,
        string institution,
        int startYear,
        int? endYear,
        string? score)
    {
        if (endYear.HasValue && endYear < startYear)
            throw new DomainValidationException("End year cannot be before start year.");

        return new Education
        {
            StudentProfileId = studentProfileId,
            Degree = degree.Trim(),
            Institution = institution.Trim(),
            StartYear = startYear,
            EndYear = endYear,
            Score = score?.Trim()
        };
    }

    public void Update(string degree, string institution, int startYear, int? endYear, string? score)
    {
        if (endYear.HasValue && endYear < startYear)
            throw new DomainValidationException("End year cannot be before start year.");

        Degree = degree.Trim();
        Institution = institution.Trim();
        StartYear = startYear;
        EndYear = endYear;
        Score = score?.Trim();
        SetUpdatedAt();
    }
}