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

        if (onPlayerConnected is null)
            throw new InvalidOperationException("OnPlayerConnected event was null");

        if (string.IsNullOrWhiteSpace(onPlayerConnected.GameType))
            throw new ArgumentException("OnPlayerConnected event contained null or empty 'GameType'", nameof(onPlayerConnected));

        if (string.IsNullOrWhiteSpace(onPlayerConnected.Guid))
            throw new ArgumentException("OnPlayerConnected event contained null or empty 'Guid'", nameof(onPlayerConnected));

        if (!Enum.TryParse(onPlayerConnected.GameType, out GameType gameType))
            throw new ArgumentException($"OnPlayerConnected event contained invalid 'GameType': {onPlayerConnected.GameType}", nameof(onPlayerConnected));

        var onPlayerConnectedTelemetry = new EventTelemetry("OnPlayerConnected");
        onPlayerConnectedTelemetry.Properties.Add("GameType", onPlayerConnected.GameType);
        onPlayerConnectedTelemetry.Properties.Add("Username", onPlayerConnected.Username);
        onPlayerConnectedTelemetry.Properties.Add("Guid", onPlayerConnected.Guid);
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

        var onChatMessageTelemetry = new EventTelemetry("OnChatMessage");
        onChatMessageTelemetry.Properties.Add("GameType", onChatMessage.GameType);
        onChatMessageTelemetry.Properties.Add("Username", onChatMessage.Username);
        onChatMessageTelemetry.Properties.Add("Guid", onChatMessage.Guid);
        onChatMessageTelemetry.Properties.Add("Message", onChatMessage.Message);
        telemetryClient.TrackEvent(onChatMessageTelemetry);

        var playerId = await GetPlayerId(gameType, onChatMessage.Guid).ConfigureAwait(false);

        if (playerId != Guid.Empty)
        {
            var chatMessage = new CreateChatMessageDto(onChatMessage.ServerId, playerId, onChatMessage.Type.ToChatType(), onChatMessage.Username, onChatMessage.Message, onChatMessage.EventGeneratedUtc);
            await repositoryApiClient.ChatMessages.V1.CreateChatMessage(chatMessage).ConfigureAwait(false);
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

        var onMapVoteTelemetry = new EventTelemetry("OnMapVote");
        onMapVoteTelemetry.Properties.Add("GameType", onMapVote.GameType);
        onMapVoteTelemetry.Properties.Add("Guid", onMapVote.Guid);
        onMapVoteTelemetry.Properties.Add("MapName", onMapVote.MapName);
        onMapVoteTelemetry.Properties.Add("Like", onMapVote.Like.ToString());
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
        var cacheKey = $"{gameType}-${guid}";

        if (memoryCache.TryGetValue(cacheKey, out Guid playerId))
            return playerId;

        var playerDtoApiResponse = await repositoryApiClient.Players.V1.GetPlayerByGameType(gameType, guid, PlayerEntityOptions.None).ConfigureAwait(false);

        if (playerDtoApiResponse.IsSuccess && playerDtoApiResponse.Result?.Data != null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
            memoryCache.Set(cacheKey, playerDtoApiResponse.Result.Data.PlayerId, cacheEntryOptions);

            return playerDtoApiResponse.Result.Data.PlayerId;
        }

        return Guid.Empty;
    }
}