using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Extensions.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.ChatMessages;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class PlayerEventsIngest(
    ILogger<PlayerEventsIngest> logger,
    IRepositoryApiClient repositoryApiClient,
    IMemoryCache memoryCache,
    TelemetryClient telemetryClient)
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

        if (onPlayerConnected == null)
            throw new Exception("OnPlayerConnected event was null");

        if (string.IsNullOrWhiteSpace(onPlayerConnected.GameType))
            throw new Exception("OnPlayerConnected event contained null or empty 'GameType'");

        if (string.IsNullOrWhiteSpace(onPlayerConnected.Guid))
            throw new Exception("OnPlayerConnected event contained null or empty 'Guid'");

        if (!Enum.TryParse(onPlayerConnected.GameType, out GameType gameType))
            throw new Exception("OnPlayerConnected event contained invalid 'GameType'");

        var onPlayerConnectedTelemetry = new EventTelemetry("OnPlayerConnected");
        onPlayerConnectedTelemetry.Properties.Add("GameType", onPlayerConnected.GameType);
        onPlayerConnectedTelemetry.Properties.Add("Username", onPlayerConnected.Username);
        onPlayerConnectedTelemetry.Properties.Add("Guid", onPlayerConnected.Guid);
        telemetryClient.TrackEvent(onPlayerConnectedTelemetry);

        var playerExistsApiResponse = await repositoryApiClient.Players.V1.HeadPlayerByGameType(gameType, onPlayerConnected.Guid);

        if (playerExistsApiResponse.IsNotFound)
        {
            var player = new CreatePlayerDto(onPlayerConnected.Username, onPlayerConnected.Guid, onPlayerConnected.GameType.ToGameType())
            {
                IpAddress = onPlayerConnected.IpAddress
            };

            await repositoryApiClient.Players.V1.CreatePlayer(player);
        }
        else
        {
            var playerId = await GetPlayerId(gameType, onPlayerConnected.Guid);
            if (playerId != Guid.Empty)
            {
                var editPlayerDto = new EditPlayerDto(playerId)
                {
                    Username = onPlayerConnected.Username,
                    IpAddress = onPlayerConnected.IpAddress
                };

                await repositoryApiClient.Players.V1.UpdatePlayer(editPlayerDto);
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

        if (onChatMessage == null)
            throw new Exception("OnChatMessage event was null");

        if (string.IsNullOrWhiteSpace(onChatMessage.GameType))
            throw new Exception("OnChatMessage event contained null or empty 'GameType'");

        if (string.IsNullOrWhiteSpace(onChatMessage.Guid))
            throw new Exception("OnChatMessage event contained null or empty 'Guid'");

        if (!Enum.TryParse(onChatMessage.GameType, out GameType gameType))
            throw new Exception("OnChatMessage event contained invalid 'GameType'");

        var onChatMessageTelemetry = new EventTelemetry("OnChatMessage");
        onChatMessageTelemetry.Properties.Add("GameType", onChatMessage.GameType);
        onChatMessageTelemetry.Properties.Add("Username", onChatMessage.Username);
        onChatMessageTelemetry.Properties.Add("Guid", onChatMessage.Guid);
        onChatMessageTelemetry.Properties.Add("Message", onChatMessage.Message);
        telemetryClient.TrackEvent(onChatMessageTelemetry);

        var playerId = await GetPlayerId(gameType, onChatMessage.Guid);

        if (playerId != Guid.Empty)
        {
            var chatMessage = new CreateChatMessageDto(onChatMessage.ServerId, playerId, onChatMessage.Type.ToChatType(), onChatMessage.Username, onChatMessage.Message, onChatMessage.EventGeneratedUtc);
            await repositoryApiClient.ChatMessages.V1.CreateChatMessage(chatMessage);
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

        if (onMapVote == null)
            throw new Exception("OnMapVote event was null");

        if (string.IsNullOrWhiteSpace(onMapVote.MapName))
            throw new Exception("OnMapVote event contained null or empty 'MapName'");

        if (string.IsNullOrWhiteSpace(onMapVote.Guid))
            throw new Exception("OnMapVote event contained null or empty 'Guid'");

        if (!Enum.TryParse(onMapVote.GameType, out GameType gameType))
            throw new Exception("OnMapVote event contained invalid 'GameType'");

        var onMapVoteTelemetry = new EventTelemetry("OnMapVote");
        onMapVoteTelemetry.Properties.Add("GameType", onMapVote.GameType);
        onMapVoteTelemetry.Properties.Add("Guid", onMapVote.Guid);
        onMapVoteTelemetry.Properties.Add("MapName", onMapVote.MapName);
        onMapVoteTelemetry.Properties.Add("Like", onMapVote.Like.ToString());
        telemetryClient.TrackEvent(onMapVoteTelemetry);

        var playerId = await GetPlayerId(gameType, onMapVote.Guid);

        if (playerId != Guid.Empty)
        {
            var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(gameType, onMapVote.MapName);

            if (mapApiResponse.IsSuccess && mapApiResponse.Result?.Data != null)
            {
                var upsertMapVoteDto = new UpsertMapVoteDto(mapApiResponse.Result.Data.MapId, playerId, onMapVote.ServerId, onMapVote.Like);
                await repositoryApiClient.Maps.V1.UpsertMapVote(upsertMapVoteDto);
            }
        }
        else
        {
            logger.LogError($"ProcessOnMapVote :: NOPLAYER :: Guid: '{onMapVote.Guid}', Map Name: '{onMapVote.MapName}', Like: '{onMapVote.Like}'");
        }
    }

    private async Task<Guid> GetPlayerId(GameType gameType, string guid)
    {
        var cacheKey = $"{gameType}-${guid}";

        if (memoryCache.TryGetValue(cacheKey, out Guid playerId))
            return playerId;

        var playerDtoApiResponse = await repositoryApiClient.Players.V1.GetPlayerByGameType(gameType, guid, PlayerEntityOptions.None);

        if (playerDtoApiResponse.IsSuccess && playerDtoApiResponse.Result?.Data != null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
            memoryCache.Set(cacheKey, playerDtoApiResponse.Result.Data.PlayerId, cacheEntryOptions);

            return playerDtoApiResponse.Result.Data.PlayerId;
        }

        return Guid.Empty;
    }
}