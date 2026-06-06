using SharedKernel.Abstractions;

namespace DriveService.Domain.Entities;

public class DriveRound : BaseEntity
{
    private DriveRound() { }

    public Guid Id { get; private set; }

    public Guid DriveId { get; private set; }

    public int RoundNumber { get; private set; }

    public string RoundName { get; private set; } = string.Empty;

    public Drive Drive { get; private set; } = null!;

    public static DriveRound Create(
        Guid driveId,
        int roundNumber,
        string roundName)
    {
        return new DriveRound
        {
            Id = Guid.NewGuid(),
            DriveId = driveId,
            RoundNumber = roundNumber,
            RoundName = roundName
        };
    }

    public void Rename(string roundName)
    {
        RoundName = roundName;
    }
}