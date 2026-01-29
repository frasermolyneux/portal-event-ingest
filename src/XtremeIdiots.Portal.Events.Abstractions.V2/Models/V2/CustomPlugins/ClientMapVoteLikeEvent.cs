using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.CustomPlugins;

/// <summary>
/// Event triggered when a player votes positively for the current map
/// </summary>
public class ClientMapVoteLikeEvent : PlayerEvent
{
    /// <summary>
    /// Map name being voted on
    /// </summary>
    [Required]
    public string MapName { get; set; } = string.Empty;
}
