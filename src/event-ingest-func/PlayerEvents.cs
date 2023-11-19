using System.Net;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventsApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventsFunc;

public class PlayerEvents
{
    private readonly IConfiguration configuration;

    public PlayerEvents(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    [Function(nameof(OnPlayerConnected))]
    public async Task<HttpResponseData> OnPlayerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
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

        await using (var client = new ServiceBusClient(configuration["service_bus_connection_string"]))
        {
            var sender = client.CreateSender("player_connected_queue");
            await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onPlayerConnected)));
        };

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnChatMessage))]
    public async Task<HttpResponseData> OnChatMessage([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
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

        await using (var client = new ServiceBusClient(configuration["service_bus_connection_string"]))
        {
            var sender = client.CreateSender("chat_message_queue");
            await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onChatMessage)));
        };

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnMapVote))]
    public async Task<HttpResponseData> OnMapVote([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
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

        await using (var client = new ServiceBusClient(configuration["service_bus_connection_string"]))
        {
            var sender = client.CreateSender("map_vote_queue");
            await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onMapVote)));
        };

        return req.CreateResponse(HttpStatusCode.OK);
    }
}