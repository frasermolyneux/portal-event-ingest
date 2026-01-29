using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.PlayerLifecycle;

/// <summary>
/// Event triggered when a player initially establishes connection to the server
/// </summary>
public class ClientConnectEvent : PlayerEvent
{
    /// <summary>
    /// Number of connections to this server
    /// </summary>
    [Required]
    public int ConnectionNumber { get; set; }
}
