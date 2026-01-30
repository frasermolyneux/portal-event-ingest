using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models;

/// <summary>
/// Base event for communication/chat events
/// </summary>
public class CommunicationEvent : PlayerEvent
{
    /// <summary>
    /// Type of communication
    /// </summary>
    [Required]
    public CommunicationType CommunicationType { get; set; }

    /// <summary>
    /// Message text content
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Message length in characters
    /// </summary>
    public int MessageLength { get; set; }

    /// <summary>
    /// Recipient GUID (for private messages)
    /// </summary>
    public string? RecipientGuid { get; set; }

    /// <summary>
    /// Recipient name (for private messages)
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// Target team (for team messages)
    /// </summary>
    public string? TargetTeam { get; set; }

    /// <summary>
    /// Squad ID (for squad messages)
    /// </summary>
    public string? SquadId { get; set; }

    /// <summary>
    /// Radio command (for radio messages)
    /// </summary>
    public string? RadioCommand { get; set; }

    /// <summary>
    /// Radio command category (for radio messages)
    /// </summary>
    public string? RadioCommandCategory { get; set; }
}
