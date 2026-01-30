using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.CustomPlugins;

/// <summary>
/// Event triggered when a player votes negatively for the current map
/// </summary>
public class ClientMapVoteDislikeEvent : PlayerEvent
{
    /// <summary>
    /// Map name being voted on
    /// </summary>
    [Required]
    public string MapName { get; set; } = string.Empty;
}
