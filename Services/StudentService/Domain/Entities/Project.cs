using SharedKernel.Abstractions;
using SharedKernel.Exceptions;
using StudentService.Domain.Events;

public class Project : BaseEntity
{
    public Guid StudentProfileId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Stored as JSON array in PostgreSQL — ["ASP.NET Core", "Docker", "Kafka"]
    /// No need to normalise since it's always fetched/updated as a unit.
    /// </summary>
    public List<string> TechStack { get; private set; } = new();
    public string? ProjectUrl { get; private set; }

    private Project() { }

    public static Project Create(
        Guid studentProfileId,
        string title,
        string description,
        List<string> techStack,
        string? projectUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainValidationException("Project title is required.");

        return new Project
        {
            StudentProfileId = studentProfileId,
            Title = title.Trim(),
            Description = description.Trim(),
            TechStack = techStack.Where(t => !string.IsNullOrWhiteSpace(t))
                                 .Select(t => t.Trim())
                                 .Distinct()
                                 .ToList(),
            ProjectUrl = projectUrl?.Trim()
        };
    }

    public void Update(string title, string description, List<string> techStack, string? projectUrl)
    {
        Title = title.Trim();
        Description = description.Trim();
        TechStack = techStack.Where(t => !string.IsNullOrWhiteSpace(t))
                             .Select(t => t.Trim())
                             .Distinct()
                             .ToList();
        ProjectUrl = projectUrl?.Trim();
        SetUpdatedAt();
    }
}