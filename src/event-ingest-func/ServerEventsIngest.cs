using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventIngestApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.EventIngestFunc;

public class ServerEventsIngest
{
    private readonly ILogger<ServerEventsIngest> logger;
    private readonly IRepositoryApiClient repositoryApiClient;

    public ServerEventsIngest(
        ILogger<ServerEventsIngest> logger,
        IRepositoryApiClient repositoryApiClient)
    {
        this.logger = logger;
        this.repositoryApiClient = repositoryApiClient;
    }

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
            logger.LogError(ex, "OnServerConnected was not in expected format");
            throw;
        }

        if (onServerConnected == null)
            throw new Exception("OnServerConnected event was null");

        if (!Guid.TryParse(onServerConnected.Id, out Guid gameServerId))
            throw new Exception("OnServerConnected event contained null or empty 'onServerConnected'");

        logger.LogInformation(
            $"OnServerConnected :: Id: '{onServerConnected.Id}', GameType: '{onServerConnected.GameType}'");

        var gameServerEvent = new CreateGameServerEventDto(gameServerId, "OnServerConnected", JsonConvert.SerializeObject(onServerConnected));
        await repositoryApiClient.GameServersEvents.V1.CreateGameServerEvent(gameServerEvent);
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
            logger.LogError(ex, "OnMapChange was not in expected format");
            throw;
        }

        if (onMapChange == null)
            throw new Exception("OnMapChange event was null");

        if (onMapChange.ServerId == Guid.Empty)
            throw new Exception("OnMapChange event contained null or empty 'ServerId'");

        logger.LogInformation(
            $"ProcessOnMapChange :: GameName: '{onMapChange.GameName}', GameType: '{onMapChange.GameType}', MapName: '{onMapChange.MapName}'");

        var gameServerEvent = new CreateGameServerEventDto(onMapChange.ServerId, "MapChange", JsonConvert.SerializeObject(onMapChange));
        await repositoryApiClient.GameServersEvents.V1.CreateGameServerEvent(gameServerEvent);
    }
}