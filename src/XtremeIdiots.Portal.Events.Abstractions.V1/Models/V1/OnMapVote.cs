namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnMapVote : OnEventBase
{
    public required string MapName { get; set; }
    public required string Guid { get; set; }
    public required bool Like { get; set; }
}