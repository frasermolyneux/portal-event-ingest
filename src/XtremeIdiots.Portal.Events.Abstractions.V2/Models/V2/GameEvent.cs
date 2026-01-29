namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2;

/// <summary>
/// Base event for game and server events
/// </summary>
public class GameEvent : BaseEvent
{
    /// <summary>
    /// Current map name
    /// </summary>
    public string? MapName { get; set; }

    /// <summary>
    /// Game mode name
    /// </summary>
    public string? GameName { get; set; }

    /// <summary>
    /// Round-specific information
    /// </summary>
    public string? RoundInfo { get; set; }

    /// <summary>
    /// Match results data
    /// </summary>
    public string? GameResults { get; set; }

    /// <summary>
    /// Player score data
    /// </summary>
    public object? ScoreData { get; set; }

    /// <summary>
    /// Previous map name (for map changes)
    /// </summary>
    public string? PreviousMap { get; set; }

    /// <summary>
    /// New map name (for map changes)
    /// </summary>
    public string? NewMap { get; set; }

    /// <summary>
    /// Warmup information
    /// </summary>
    public string? WarmupInfo { get; set; }
}
