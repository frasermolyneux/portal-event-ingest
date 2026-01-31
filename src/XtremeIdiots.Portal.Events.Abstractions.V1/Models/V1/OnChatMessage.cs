namespace XtremeIdiots.Portal.Events.Abstractions.Models.V1;

public class OnChatMessage : OnEventBase
{
    public required string Username { get; init; }
    public required string Guid { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; }
}