using XtremeIdiots.Portal.Events.Abstractions.Models;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

public static class EventIngestDtoFactory
{
    public static ApiInfoDto CreateApiInfo(
        string version = "1.0.0+abc123",
        string buildVersion = "1.0.0",
        string assemblyVersion = "1.0.0.0")
    {
        return new ApiInfoDto
        {
            Version = version,
            BuildVersion = buildVersion,
            AssemblyVersion = assemblyVersion
        };
    }

    public static OnPlayerConnected CreateOnPlayerConnected(
        string? username = null,
        string? guid = null,
        string? ipAddress = null,
        string? gameType = null,
        Guid? serverId = null,
        DateTime? eventGeneratedUtc = null)
    {
        return new OnPlayerConnected
        {
            Username = username ?? "TestPlayer",
            Guid = guid ?? System.Guid.NewGuid().ToString(),
            IpAddress = ipAddress ?? "127.0.0.1",
            GameType = gameType ?? "CallOfDuty4",
            ServerId = serverId ?? System.Guid.NewGuid(),
            EventGeneratedUtc = eventGeneratedUtc ?? DateTime.UtcNow
        };
    }

    public static OnChatMessage CreateOnChatMessage(
        string? username = null,
        string? guid = null,
        string? message = null,
        string? type = null,
        string? gameType = null,
        Guid? serverId = null,
        DateTime? eventGeneratedUtc = null)
    {
        return new OnChatMessage
        {
            Username = username ?? "TestPlayer",
            Guid = guid ?? System.Guid.NewGuid().ToString(),
            Message = message ?? "Hello world",
            Type = type ?? "say",
            GameType = gameType ?? "CallOfDuty4",
            ServerId = serverId ?? System.Guid.NewGuid(),
            EventGeneratedUtc = eventGeneratedUtc ?? DateTime.UtcNow
        };
    }

    public static OnMapVote CreateOnMapVote(
        string? mapName = null,
        string? guid = null,
        bool like = true,
        string? gameType = null,
        Guid? serverId = null,
        DateTime? eventGeneratedUtc = null)
    {
        return new OnMapVote
        {
            MapName = mapName ?? "mp_crossfire",
            Guid = guid ?? System.Guid.NewGuid().ToString(),
            Like = like,
            GameType = gameType ?? "CallOfDuty4",
            ServerId = serverId ?? System.Guid.NewGuid(),
            EventGeneratedUtc = eventGeneratedUtc ?? DateTime.UtcNow
        };
    }

    public static OnServerConnected CreateOnServerConnected(
        string? id = null,
        string? gameType = null)
    {
        return new OnServerConnected
        {
            Id = id ?? System.Guid.NewGuid().ToString(),
            GameType = gameType ?? "CallOfDuty4"
        };
    }

    public static OnMapChange CreateOnMapChange(
        string? gameName = null,
        string? mapName = null,
        string? gameType = null,
        Guid? serverId = null,
        DateTime? eventGeneratedUtc = null)
    {
        return new OnMapChange
        {
            GameName = gameName ?? "Call of Duty 4",
            MapName = mapName ?? "mp_crossfire",
            GameType = gameType ?? "CallOfDuty4",
            ServerId = serverId ?? System.Guid.NewGuid(),
            EventGeneratedUtc = eventGeneratedUtc ?? DateTime.UtcNow
        };
    }
}
