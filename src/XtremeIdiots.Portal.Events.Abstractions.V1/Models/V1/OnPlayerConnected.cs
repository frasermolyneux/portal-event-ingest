namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnPlayerConnected : OnEventBase
{
    public required string Username { get; set; }
    public required string Guid { get; set; }
    public required string IpAddress { get; set; }
}