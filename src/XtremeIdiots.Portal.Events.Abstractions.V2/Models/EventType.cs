namespace XtremeIdiots.Portal.Events.Abstractions.V2.Models;

/// <summary>
/// Event types supported by the V2 API
/// </summary>
public enum EventType
{
    // Player Lifecycle Events
    ClientConnect,
    ClientAuth,
    ClientJoin,
    ClientDisconnect,
    ClientNameChange,
    
    // Communication Events
    ClientSay,
    ClientTeamSay,
    ClientSquadSay,
    ClientPrivateSay,
    ClientRadio,
    
    // Custom Plugin Events
    ClientMapVoteLike,
    ClientMapVoteDislike,
    
    // Server and Game Events
    ServerStartup,
    GameMapChange,
    GameRoundStart,
    GameRoundEnd,
    GameExit,
    GameWarmup,
    GameRoundPlayerScores
}
