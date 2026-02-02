namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnPlayerConnected : OnEventBase
{
    public required string Username { get; init; }
    public required string Guid { get; init; }
    public required string IpAddress { get; init; }
}