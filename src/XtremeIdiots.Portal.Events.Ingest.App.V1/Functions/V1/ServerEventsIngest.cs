using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ServerEventsIngest(
    ILogger<ServerEventsIngest> logger,
    IRepositoryApiClient repositoryApiClient)
{
    [Function("ProcessOnServerConnected")]
    public async Task ProcessOnServerConnected(
        [ServiceBusTrigger("server_connected_queue", Connection = "ServiceBusConnection")]
        string myQueueItem)
    {
        OnServerConnected? onServerConnected;
        try
        {
            onServerConnected = JsonConvert.DeserializeObject<OnServerConnected>(myQueueItem);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OnServerConnected was not in expected format. Payload: {Payload}", myQueueItem);
            return;
        }

        if (onServerConnected is null)
        {
            logger.LogWarning("OnServerConnected deserialized to null. Payload: {Payload}", myQueueItem);
            return;
        }

        if (!Guid.TryParse(onServerConnected.Id, out Guid gameServerId))
        {
            logger.LogWarning("OnServerConnected event contained invalid or empty 'Id'. Payload: {Payload}", myQueueItem);
            return;
        }

        logger.LogInformation(
            "OnServerConnected :: Id: '{Id}', GameType: '{GameType}'", onServerConnected.Id, onServerConnected.GameType);

        var gameServerEvent = new CreateGameServerEventDto(gameServerId, "OnServerConnected", JsonConvert.SerializeObject(onServerConnected));
        await repositoryApiClient.GameServersEvents.V1.CreateGameServerEvent(gameServerEvent).ConfigureAwait(false);
    }

    [Function("ProcessOnMapChange")]
    public async Task ProcessOnMapChange(
        [ServiceBusTrigger("map_change_queue", Connection = "ServiceBusConnection")]
        string myQueueItem)
    {
        OnMapChange? onMapChange;
        try
        {
            onMapChange = JsonConvert.DeserializeObject<OnMapChange>(myQueueItem);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OnMapChange was not in expected format. Payload: {Payload}", myQueueItem);
            return;
        }

        if (onMapChange is null)
        {
            logger.LogWarning("OnMapChange deserialized to null. Payload: {Payload}", myQueueItem);
            return;
        }

        if (onMapChange.ServerId == Guid.Empty)
        {
            logger.LogWarning("OnMapChange event contained null or empty 'ServerId'. Payload: {Payload}", myQueueItem);
            return;
        }

        logger.LogInformation(
            "ProcessOnMapChange :: GameName: '{GameName}', GameType: '{GameType}', MapName: '{MapName}'", onMapChange.GameName, onMapChange.GameType, onMapChange.MapName);

        var gameServerEvent = new CreateGameServerEventDto(onMapChange.ServerId, "MapChange", JsonConvert.SerializeObject(onMapChange));
        await repositoryApiClient.GameServersEvents.V1.CreateGameServerEvent(gameServerEvent).ConfigureAwait(false);
    }
}