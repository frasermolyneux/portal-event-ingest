using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Extensions.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.ChatMessages;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class PlayerEventsIngest(
    ILogger<PlayerEventsIngest> logger,
    IRepositoryApiClient repositoryApiClient,
    IMemoryCache memoryCache,
    TelemetryClient telemetryClient,
    IConfiguration configuration,
    IFeatureManager featureManager,
    ILocalWordListFilter localWordListFilter,
    IChatModerationService chatModerationService)
{
    [Function("ProcessOnPlayerConnected")]
    public async Task ProcessOnPlayerConnected(
        [ServiceBusTrigger("player_connected_queue", Connection = "ServiceBusConnection")]
        string myQueueItem)
    {
        OnPlayerConnected? onPlayerConnected;
        try
        {
            onPlayerConnected = JsonConvert.DeserializeObject<OnPlayerConnected>(myQueueItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OnPlayerConnected was not in expected format");
            throw;
        }

        if (onPlayerConnected is null)
            throw new InvalidOperationException("OnPlayerConnected event was null");

        if (string.IsNullOrWhiteSpace(onPlayerConnected.GameType))
            throw new ArgumentException("OnPlayerConnected event contained null or empty 'GameType'", nameof(onPlayerConnected));

        if (string.IsNullOrWhiteSpace(onPlayerConnected.Guid))
            throw new ArgumentException("OnPlayerConnected event contained null or empty 'Guid'", nameof(onPlayerConnected));

        if (!Enum.TryParse(onPlayerConnected.GameType, out GameType gameType))
            throw new ArgumentException($"OnPlayerConnected event contained invalid 'GameType': {onPlayerConnected.GameType}", nameof(onPlayerConnected));

        var onPlayerConnectedTelemetry = new EventTelemetry("OnPlayerConnected")
        {
            Properties =
            {
                ["GameType"] = onPlayerConnected.GameType,
                ["Username"] = onPlayerConnected.Username,
                ["Guid"] = onPlayerConnected.Guid
            }
        };
        telemetryClient.TrackEvent(onPlayerConnectedTelemetry);

        var playerExistsApiResponse = await repositoryApiClient.Players.V1.HeadPlayerByGameType(gameType, onPlayerConnected.Guid).ConfigureAwait(false);

        if (playerExistsApiResponse.IsNotFound)
        {
            var player = new CreatePlayerDto(onPlayerConnected.Username, onPlayerConnected.Guid, onPlayerConnected.GameType.ToGameType())
            {
                IpAddress = onPlayerConnected.IpAddress
            };

            await repositoryApiClient.Players.V1.CreatePlayer(player).ConfigureAwait(false);
        }
        else
        {
            var playerId = await GetPlayerId(gameType, onPlayerConnected.Guid).ConfigureAwait(false);
            if (playerId != Guid.Empty)
            {
                var editPlayerDto = new EditPlayerDto(playerId)
                {
                    Username = onPlayerConnected.Username,
                    IpAddress = onPlayerConnected.IpAddress
                };

                await repositoryApiClient.Players.V1.UpdatePlayer(editPlayerDto).ConfigureAwait(false);
            }
        }
    }

    [Function("ProcessOnChatMessage")]
    public async Task ProcessOnChatMessage(
        [ServiceBusTrigger("chat_message_queue", Connection = "ServiceBusConnection")]
        string myQueueItem)
    {
        OnChatMessage? onChatMessage;
        try
        {
            onChatMessage = JsonConvert.DeserializeObject<OnChatMessage>(myQueueItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OnChatMessage was not in expected format");
            throw;
        }

        if (onChatMessage is null)
            throw new InvalidOperationException("OnChatMessage event was null");

        if (string.IsNullOrWhiteSpace(onChatMessage.GameType))
            throw new ArgumentException("OnChatMessage event contained null or empty 'GameType'", nameof(onChatMessage));

        if (string.IsNullOrWhiteSpace(onChatMessage.Guid))
            throw new ArgumentException("OnChatMessage event contained null or empty 'Guid'", nameof(onChatMessage));

        if (!Enum.TryParse(onChatMessage.GameType, out GameType gameType))
            throw new ArgumentException($"OnChatMessage event contained invalid 'GameType': {onChatMessage.GameType}", nameof(onChatMessage));

        var onChatMessageTelemetry = new EventTelemetry("OnChatMessage")
        {
            Properties =
            {
                ["GameType"] = onChatMessage.GameType,
                ["Username"] = onChatMessage.Username,
                ["Guid"] = onChatMessage.Guid,
                ["Message"] = onChatMessage.Message
            }
        };
        telemetryClient.TrackEvent(onChatMessageTelemetry);

        var playerContext = await GetPlayerContext(gameType, onChatMessage.Guid).ConfigureAwait(false);

        if (playerContext is not null)
        {
            var chatMessage = new CreateChatMessageDto(onChatMessage.ServerId, playerContext.PlayerId, onChatMessage.Type.ToChatType(), onChatMessage.Username, onChatMessage.Message, onChatMessage.EventGeneratedUtc);
            await repositoryApiClient.ChatMessages.V1.CreateChatMessage(chatMessage).ConfigureAwait(false);

            if (await featureManager.IsEnabledAsync("ChatToxicityDetection"))
            {
                await RunModerationPipeline(onChatMessage, playerContext, gameType).ConfigureAwait(false);
            }
        }
        else
        {
            logger.LogError($"ProcessOnChatMessage :: NOPLAYER :: Username: '{onChatMessage.Username}', Guid: '{onChatMessage.Guid}', Message: '{onChatMessage.Message}', Timestamp: '{onChatMessage.EventGeneratedUtc}'");
        }
    }

    [Function("ProcessOnMapVote")]
    public async Task ProcessOnMapVote(
     [ServiceBusTrigger("map_vote_queue", Connection = "ServiceBusConnection")]
        string myQueueItem)
    {
        OnMapVote? onMapVote;
        try
        {
            onMapVote = JsonConvert.DeserializeObject<OnMapVote>(myQueueItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OnMapVote was not in expected format");
            throw;
        }

        if (onMapVote is null)
            throw new InvalidOperationException("OnMapVote event was null");

        if (string.IsNullOrWhiteSpace(onMapVote.MapName))
            throw new ArgumentException("OnMapVote event contained null or empty 'MapName'", nameof(onMapVote));

        if (string.IsNullOrWhiteSpace(onMapVote.Guid))
            throw new ArgumentException("OnMapVote event contained null or empty 'Guid'", nameof(onMapVote));

        if (!Enum.TryParse(onMapVote.GameType, out GameType gameType))
            throw new ArgumentException($"OnMapVote event contained invalid 'GameType': {onMapVote.GameType}", nameof(onMapVote));

        var onMapVoteTelemetry = new EventTelemetry("OnMapVote")
        {
            Properties =
            {
                ["GameType"] = onMapVote.GameType,
                ["Guid"] = onMapVote.Guid,
                ["MapName"] = onMapVote.MapName,
                ["Like"] = onMapVote.Like.ToString()
            }
        };
        telemetryClient.TrackEvent(onMapVoteTelemetry);

        var playerId = await GetPlayerId(gameType, onMapVote.Guid).ConfigureAwait(false);

        if (playerId != Guid.Empty)
        {
            var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(gameType, onMapVote.MapName).ConfigureAwait(false);

            if (mapApiResponse.IsSuccess && mapApiResponse.Result?.Data != null)
            {
                var upsertMapVoteDto = new UpsertMapVoteDto(mapApiResponse.Result.Data.MapId, playerId, onMapVote.ServerId, onMapVote.Like);
                await repositoryApiClient.Maps.V1.UpsertMapVote(upsertMapVoteDto).ConfigureAwait(false);
            }
        }
        else
        {
            logger.LogError($"ProcessOnMapVote :: NOPLAYER :: Guid: '{onMapVote.Guid}', Map Name: '{onMapVote.MapName}', Like: '{onMapVote.Like}'");
        }
    }

    private async ValueTask<Guid> GetPlayerId(GameType gameType, string guid)
    {
        var ctx = await GetPlayerContext(gameType, guid).ConfigureAwait(false);
        return ctx?.PlayerId ?? Guid.Empty;
    }

    private async ValueTask<PlayerContext?> GetPlayerContext(GameType gameType, string guid)
    {
        var cacheKey = $"player-ctx-{gameType}-{guid}";

        if (memoryCache.TryGetValue(cacheKey, out PlayerContext? cached))
            return cached;

        var playerDtoApiResponse = await repositoryApiClient.Players.V1
            .GetPlayerByGameType(gameType, guid, PlayerEntityOptions.Tags).ConfigureAwait(false);

        if (!playerDtoApiResponse.IsSuccess || playerDtoApiResponse.Result?.Data is null)
            return null;

        var player = playerDtoApiResponse.Result.Data;
        var moderateTagName = configuration["ContentSafety:ModerateChatTagName"] ?? "moderate-chat";
        var hasTag = player.Tags.Any(t =>
            string.Equals(t.Tag?.Name, moderateTagName, StringComparison.OrdinalIgnoreCase));

        var ctx = new PlayerContext(player.PlayerId, player.FirstSeen, hasTag);

        memoryCache.Set(cacheKey, ctx,
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));

        return ctx;
    }

    private async Task RunModerationPipeline(
        OnChatMessage chatMessage, PlayerContext player, GameType gameType)
    {
        var minLength = int.Parse(configuration["ContentSafety:MinMessageLength"] ?? "5");

        if (chatMessage.Message.Length < minLength) return;
        if (chatMessage.Message.StartsWith("QUICKMESSAGE_", StringComparison.OrdinalIgnoreCase)) return;

        // Tier 1: Local word list (free, always runs)
        var localMatch = localWordListFilter.Check(chatMessage.Message);
        if (localMatch is not null)
        {
            await CreateModerationAdminAction(
                player.PlayerId, gameType, chatMessage,
                source: "Word Filter",
                reason: $"[Word Filter] {localMatch.MatchedCategory} detected. " +
                        $"Message: \"{Truncate(chatMessage.Message, 200)}\"");
            return;
        }

        // Tier 2: Content Safety API (paid, only for qualifying players)
        var newPlayerDays = int.Parse(configuration["ContentSafety:NewPlayerWindowDays"] ?? "7");
        var isNewPlayer = newPlayerDays > 0
            && player.FirstSeen > DateTime.UtcNow.AddDays(-newPlayerDays);

        if (!isNewPlayer && !player.HasModerateChatTag)
            return;

        var result = await chatModerationService.AnalyseAsync(chatMessage.Message).ConfigureAwait(false);
        var threshold = int.Parse(configuration["ContentSafety:SeverityThreshold"] ?? "4");

        if (result is null || result.MaxSeverity < threshold)
            return;

        await CreateModerationAdminAction(
            player.PlayerId, gameType, chatMessage,
            source: "AI Content Safety",
            reason: $"[AI Content Safety] {result.Category} (severity {result.MaxSeverity}/6). " +
                    $"Message: \"{Truncate(chatMessage.Message, 200)}\" | " +
                    $"Scores: Hate={result.HateSeverity}, Violence={result.ViolenceSeverity}, " +
                    $"Sexual={result.SexualSeverity}, SelfHarm={result.SelfHarmSeverity}");
    }

    private async Task CreateModerationAdminAction(
        Guid playerId, GameType gameType, OnChatMessage chatMessage,
        string source, string reason)
    {
        telemetryClient.TrackEvent("ChatModerationTriggered", new Dictionary<string, string>
        {
            ["GameType"] = gameType.ToString(),
            ["ServerId"] = chatMessage.ServerId.ToString(),
            ["Source"] = source,
            ["Username"] = chatMessage.Username
        });

        var botAdminId = configuration["ContentSafety:BotAdminId"];

        var adminAction = new CreateAdminActionDto(playerId, AdminActionType.Observation, reason)
        {
            AdminId = botAdminId
        };

        await repositoryApiClient.AdminActions.V1.CreateAdminAction(adminAction).ConfigureAwait(false);

        logger.LogInformation(
            "Chat moderation triggered for player {PlayerId} via {Source}",
            playerId, source);
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}