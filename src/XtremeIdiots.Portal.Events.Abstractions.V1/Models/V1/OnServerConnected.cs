namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnServerConnected
{
    public required string Id { get; init; }
    public required string GameType { get; init; }
}