namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.PlayerLifecycle;

/// <summary>
/// Event triggered when a player disconnects from the server
/// </summary>
public class ClientDisconnectEvent : PlayerEvent
{
    /// <summary>
    /// Session duration in seconds
    /// </summary>
    public int? SessionDurationSeconds { get; set; }
}
