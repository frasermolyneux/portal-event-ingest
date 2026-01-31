namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnServerConnected
{
    public required string Id { get; set; }
    public required string GameType { get; set; }
}