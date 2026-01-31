namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnEventBase
{
    public required DateTime EventGeneratedUtc { get; set; }
    public required string GameType { get; set; }
    public required Guid ServerId { get; set; }
}