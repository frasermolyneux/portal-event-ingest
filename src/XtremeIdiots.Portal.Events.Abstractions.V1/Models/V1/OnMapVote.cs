namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnMapVote : OnEventBase
{
    public required string MapName { get; init; }
    public required string Guid { get; init; }
    public required bool Like { get; init; }
}