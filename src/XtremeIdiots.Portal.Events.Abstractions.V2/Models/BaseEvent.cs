using System.ComponentModel.DataAnnotations;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models;

/// <summary>
/// Base event containing common fields for all events
/// </summary>
public class BaseEvent
{
    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    [Required]
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// UTC timestamp when the event was generated
    /// </summary>
    [Required]
    public DateTime EventGeneratedUtc { get; set; }

    /// <summary>
    /// Type of event
    /// </summary>
    [Required]
    public EventType EventType { get; set; }

    /// <summary>
    /// Server identifier
    /// </summary>
    [Required]
    public Guid ServerId { get; set; }

    /// <summary>
    /// Game type (uses portal's canonical GameType enum)
    /// </summary>
    [Required]
    public GameType GameType { get; set; }
}
