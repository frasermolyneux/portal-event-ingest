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
            logger.LogWarning(ex, "OnPlayerConnected was not in expected format. Payload: {Payload}", myQueueItem);
            return;
        }

        if (onPlayerConnected is null)
        {
            logger.LogWarning("OnPlayerConnected deserialized to null. Payload: {Payload}", myQueueItem);
            return;
        }

        if (string.IsNullOrWhiteSpace(onPlayerConnected.GameType))
        {
            logger.LogWarning("OnPlayerConnected event contained null or empty 'GameType'. Payload: {Payload}", myQueueItem);
            return;
        }

        if (string.IsNullOrWhiteSpace(onPlayerConnected.Guid))
        {
            logger.LogWarning("OnPlayerConnected event contained null or empty 'Guid'. Payload: {Payload}", myQueueItem);
            return;
        }

        if (!Enum.TryParse(onPlayerConnected.GameType, out GameType gameType))
        {
            logger.LogWarning("OnPlayerConnected event contained invalid 'GameType': {GameType}. Payload: {Payload}", onPlayerConnected.GameType, myQueueItem);
            return;
        }

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

            var createResult = await repositoryApiClient.Players.V1.CreatePlayer(player).ConfigureAwait(false);

            if (createResult.IsConflict)
            {
                // Player was created by another event between the HEAD check and CreatePlayer call.
                // Fall through to update path.
            }
            else if (createResult.IsSuccess)
            {
                return;
            }
            else
            {
                throw new InvalidOperationException($"Failed to create player for Guid '{onPlayerConnected.Guid}'. API returned {createResult.StatusCode}.");
            }
        }

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
            logger.LogWarning(ex, "OnChatMessage was not in expected format. Payload: {Payload}", myQueueItem);
            return;
        }

        if (onChatMessage is null)
        {
            logger.LogWarning("OnChatMessage deserialized to null. Payload: {Payload}", myQueueItem);
            return;
        }

        if (string.IsNullOrWhiteSpace(onChatMessage.GameType))
        {
            logger.LogWarning("OnChatMessage event contained null or empty 'GameType'. Payload: {Payload}", myQueueItem);
            return;
        }

        if (string.IsNullOrWhiteSpace(onChatMessage.Guid))
        {
            logger.LogWarning("OnChatMessage event contained null or empty 'Guid'. Payload: {Payload}", myQueueItem);
            return;
        }

        if (!Enum.TryParse(onChatMessage.GameType, out GameType gameType))
        {
            logger.LogWarning("OnChatMessage event contained invalid 'GameType': {GameType}. Payload: {Payload}", onChatMessage.GameType, myQueueItem);
            return;
        }

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

        if (playerContext is null)
        {
            logger.LogWarning("ProcessOnChatMessage :: Player not found, message will retry. Username: '{Username}', Guid: '{Guid}'", onChatMessage.Username, onChatMessage.Guid);
            throw new InvalidOperationException($"Player not found for Guid '{onChatMessage.Guid}'. Message will retry.");
        }

        var chatMessage = new CreateChatMessageDto(onChatMessage.ServerId, playerContext.PlayerId, onChatMessage.Type.ToChatType(), onChatMessage.Username, onChatMessage.Message, onChatMessage.EventGeneratedUtc);
        await repositoryApiClient.ChatMessages.V1.CreateChatMessage(chatMessage).ConfigureAwait(false);

        if (await featureManager.IsEnabledAsync("ChatToxicityDetection"))
        {
            try
            {
                await RunModerationPipeline(onChatMessage, playerContext, gameType).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Moderation pipeline failed for chat message. Processing continues. Guid: {Guid}", onChatMessage.Guid);
            }
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
            logger.LogWarning(ex, "OnMapVote was not in expected format. Payload: {Payload}", myQueueItem);
            return;
        }

        if (onMapVote is null)
        {
            logger.LogWarning("OnMapVote deserialized to null. Payload: {Payload}", myQueueItem);
            return;
        }

        if (string.IsNullOrWhiteSpace(onMapVote.MapName))
        {
            logger.LogWarning("OnMapVote event contained null or empty 'MapName'. Payload: {Payload}", myQueueItem);
            return;
        }

        if (string.IsNullOrWhiteSpace(onMapVote.Guid))
        {
            logger.LogWarning("OnMapVote event contained null or empty 'Guid'. Payload: {Payload}", myQueueItem);
            return;
        }

        if (!Enum.TryParse(onMapVote.GameType, out GameType gameType))
        {
            logger.LogWarning("OnMapVote event contained invalid 'GameType': {GameType}. Payload: {Payload}", onMapVote.GameType, myQueueItem);
            return;
        }

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

        if (playerId == Guid.Empty)
        {
            logger.LogWarning("ProcessOnMapVote :: Player not found, message will retry. Guid: '{Guid}', MapName: '{MapName}'", onMapVote.Guid, onMapVote.MapName);
            throw new InvalidOperationException($"Player not found for Guid '{onMapVote.Guid}'. Message will retry.");
        }

        var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(gameType, onMapVote.MapName).ConfigureAwait(false);

        if (mapApiResponse.IsSuccess && mapApiResponse.Result?.Data != null)
        {
            var upsertMapVoteDto = new UpsertMapVoteDto(mapApiResponse.Result.Data.MapId, playerId, onMapVote.ServerId, onMapVote.Like);
            await repositoryApiClient.Maps.V1.UpsertMapVote(upsertMapVoteDto).ConfigureAwait(false);
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