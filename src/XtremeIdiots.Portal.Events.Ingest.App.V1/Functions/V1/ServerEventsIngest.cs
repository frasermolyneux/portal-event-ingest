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
            logger.LogError(ex, "OnServerConnected was not in expected format");
            throw;
        }

        if (onServerConnected is null)
            throw new InvalidOperationException("OnServerConnected event was null");

        if (!Guid.TryParse(onServerConnected.Id, out Guid gameServerId))
            throw new ArgumentException("OnServerConnected event contained invalid or empty 'Id'", nameof(onServerConnected));

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

        if (onMapChange is null)
            throw new InvalidOperationException("OnMapChange event was null");

        if (onMapChange.ServerId == Guid.Empty)
            throw new ArgumentException("OnMapChange event contained null or empty 'ServerId'", nameof(onMapChange));

        logger.LogInformation(
            $"ProcessOnMapChange :: GameName: '{onMapChange.GameName}', GameType: '{onMapChange.GameType}', MapName: '{onMapChange.MapName}'");

        var gameServerEvent = new CreateGameServerEventDto(onMapChange.ServerId, "MapChange", JsonConvert.SerializeObject(onMapChange));
        await repositoryApiClient.GameServersEvents.V1.CreateGameServerEvent(gameServerEvent);
    }
}