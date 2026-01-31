namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnMapChange : OnEventBase
{
    public required string GameName { get; set; }
    public required string MapName { get; set; }
}