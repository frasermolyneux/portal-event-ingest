using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class PlayerEvents
{
    private readonly ILogger<PlayerEvents> logger;

    public PlayerEvents(ILogger<PlayerEvents> logger)
    {
        this.logger = logger;
    }

    [Function("OnPlayerConnected")]
    [ServiceBusOutput("player_connected_queue", Connection = "service_bus_connection_string")]
    public string OnPlayerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input)
    {
        OnPlayerConnected onPlayerConnected;
        try
        {
            onPlayerConnected = JsonConvert.DeserializeObject<OnPlayerConnected>(input);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnPlayerConnected Raw Input: '{input}'");
            logger.LogError(ex, "OnPlayerConnected was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onPlayerConnected);
    }

    [Function("OnChatMessage")]
    [ServiceBusOutput("chat_message_queue", Connection = "service_bus_connection_string")]
    public static string OnChatMessage([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input)
    {
        OnChatMessage onChatMessage;
        try
        {
            onChatMessage = JsonConvert.DeserializeObject<OnChatMessage>(input);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnChatMessage Raw Input: '{input}'");
            logger.LogError(ex, "OnChatMessage was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onChatMessage);
    }

    [Function("OnMapVote")]
    [ServiceBusOutput("map_vote_queue", Connection = "service_bus_connection_string")]
    public static string OnMapVote([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] string input)
    {
        OnMapVote onMapVote;
        try
        {
            onMapVote = JsonConvert.DeserializeObject<OnMapVote>(input);
        }
        catch (Exception ex)
        {
            logger.LogError($"OnMapVote Raw Input: '{input}'");
            logger.LogError(ex, "OnMapVote was not in expected format");
            throw;
        }

        return JsonConvert.SerializeObject(onMapVote);
    }
}