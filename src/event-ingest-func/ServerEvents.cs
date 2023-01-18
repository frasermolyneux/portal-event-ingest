using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class ServerEvents
{
    [Function("OnServerConnected")]
    [ServiceBusOutput("server_connected_queue", Connection = "service_bus_connection_string")]
    public static async Task<string> OnServerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnPlayerConnected));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        OnServerConnected? onServerConnected;
        try
        {
            onServerConnected = JsonConvert.DeserializeObject<OnServerConnected>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnServerConnected Raw Input: '{requestBody}'");
            logger.LogError(ex, "OnServerConnected was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onServerConnected);
    }

    [Function("OnMapChange")]
    [ServiceBusOutput("map_change_queue", Connection = "service_bus_connection_string")]
    public static async Task<string> OnMapChange([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnPlayerConnected));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        OnMapChange? onMapChange;
        try
        {
            onMapChange = JsonConvert.DeserializeObject<OnMapChange>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnMapChange Raw Input: '{requestBody}'");
            logger.LogError(ex, "OnMapChange was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onMapChange);
    }
}