using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class PlayerEvents
{
    [Function(nameof(OnPlayerConnected))]
    [ServiceBusOutput("player_connected_queue", Connection = "service_bus_connection_string")]
    public static async Task<string> OnPlayerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnPlayerConnected));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        OnPlayerConnected? onPlayerConnected;
        try
        {
            onPlayerConnected = JsonConvert.DeserializeObject<OnPlayerConnected>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnPlayerConnected Raw Input: '{requestBody}'");
            logger.LogError(ex, "OnPlayerConnected was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onPlayerConnected);
    }

    [Function(nameof(OnChatMessage))]
    [ServiceBusOutput("chat_message_queue", Connection = "service_bus_connection_string")]
    public static async Task<string> OnChatMessage([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnChatMessage));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        OnChatMessage? onChatMessage;
        try
        {
            onChatMessage = JsonConvert.DeserializeObject<OnChatMessage>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnChatMessage Raw Input: '{requestBody}'");
            logger.LogError(ex, "OnChatMessage was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onChatMessage);
    }

    [Function(nameof(OnMapVote))]
    [ServiceBusOutput("map_vote_queue", Connection = "service_bus_connection_string")]
    public static async Task<string> OnMapVote([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnMapVote));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        OnMapVote? onMapVote;
        try
        {
            onMapVote = JsonConvert.DeserializeObject<OnMapVote>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnMapVote Raw Input: '{requestBody}'");
            logger.LogError(ex, "OnMapVote was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onMapVote);
    }
}