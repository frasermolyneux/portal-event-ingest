using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class ServerEvents
{
    private readonly ILogger<ServerEvents> logger;

    public ServerEvents(ILogger<ServerEvents> logger)
    {
        this.logger = logger;
    }

    [Function("OnServerConnected")]
    [ServiceBusOutput("server_connected_queue", Connection = "service_bus_connection_string")]
    public string OnServerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input)
    {
        OnServerConnected onServerConnected;
        try
        {
            onServerConnected = JsonConvert.DeserializeObject<OnServerConnected>(input);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnServerConnected Raw Input: '{input}'");
            logger.LogError(ex, "OnServerConnected was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onServerConnected);
    }

    [Function("OnMapChange")]
    [ServiceBusOutput("map_change_queue", Connection = "service_bus_connection_string")]
    public static string OnMapChange([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input)
    {
        OnMapChange onMapChange;
        try
        {
            onMapChange = JsonConvert.DeserializeObject<OnMapChange>(input);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnMapChange Raw Input: '{input}'");
            logger.LogError(ex, "OnMapChange was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onMapChange);
    }
}