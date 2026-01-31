namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnEventBase
{
    public required DateTime EventGeneratedUtc { get; init; }
    public required string GameType { get; init; }
    public required Guid ServerId { get; init; }
}