using SharedKernel.Abstractions;
using SharedKernel.Exceptions;
using StudentService.Domain.Events;

public class Skill : BaseEntity
{
    public Guid StudentProfileId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Skill() { }

    public static Skill Create(Guid studentProfileId, string name)
        => new()
        {
            StudentProfileId = studentProfileId,
            Name = name.Trim()
        };
}