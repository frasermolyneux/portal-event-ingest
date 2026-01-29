using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.CustomPlugins;

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

    /// <summary>
    /// Vote type (always "DISLIKE" for this event)
    /// </summary>
    [Required]
    public string VoteType { get; set; } = "DISLIKE";

    /// <summary>
    /// Whether this is a like vote (always false for this event)
    /// </summary>
    [Required]
    public bool Like { get; set; } = false;
}
