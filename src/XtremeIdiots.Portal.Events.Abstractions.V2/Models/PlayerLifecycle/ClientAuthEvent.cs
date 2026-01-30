namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.PlayerLifecycle;

/// <summary>
/// Event triggered when a player successfully authenticates
/// </summary>
public class ClientAuthEvent : PlayerEvent
{
    /// <summary>
    /// Player's database ID (if registered)
    /// </summary>
    public int? PlayerDbId { get; set; }

    /// <summary>
    /// Player's admin level
    /// </summary>
    public int? PlayerLevel { get; set; }
}
