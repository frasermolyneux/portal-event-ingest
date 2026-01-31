namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnMapChange : OnEventBase
{
    public required string GameName { get; init; }
    public required string MapName { get; init; }
}