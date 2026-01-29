using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.PlayerLifecycle;

/// <summary>
/// Event triggered when a player changes their name
/// </summary>
public class ClientNameChangeEvent : PlayerEvent
{
    /// <summary>
    /// Previous player name
    /// </summary>
    public string? PreviousName { get; set; }

    /// <summary>
    /// New player name
    /// </summary>
    [Required]
    public string NewName { get; set; } = string.Empty;
}
