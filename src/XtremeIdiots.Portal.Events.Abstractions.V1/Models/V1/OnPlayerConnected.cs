namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnPlayerConnected : OnEventBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Username { get; set; }
    public string Guid { get; set; }
    public string IpAddress { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}