namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnChatMessage : OnEventBase
{
    public required string Username { get; set; }
    public required string Guid { get; set; }
    public required string Message { get; set; }
    public required string Type { get; set; }
}