using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2;

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
    /// Game type
    /// </summary>
    [Required]
    public GameType GameType { get; set; }
}
