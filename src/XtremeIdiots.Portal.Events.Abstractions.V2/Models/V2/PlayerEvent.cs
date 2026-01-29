using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2;

/// <summary>
/// Base event for events involving a player
/// </summary>
public class PlayerEvent : BaseEvent
{
    /// <summary>
    /// Player GUID (primary identifier)
    /// </summary>
    [Required]
    public string PlayerGuid { get; set; } = string.Empty;

    /// <summary>
    /// Player name at time of event
    /// </summary>
    [Required]
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Player IP address (optional for privacy)
    /// </summary>
    public string? PlayerIpAddress { get; set; }

    /// <summary>
    /// Player team (if applicable)
    /// </summary>
    public string? PlayerTeam { get; set; }
}
