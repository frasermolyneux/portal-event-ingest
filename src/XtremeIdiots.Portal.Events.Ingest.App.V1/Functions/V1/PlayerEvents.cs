using System.Net;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class PlayerEvents(IServiceBusClientFactory serviceBusClientFactory)
{
    [Function(nameof(OnPlayerConnected))]
    public async Task<HttpResponseData> OnPlayerConnected([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/OnPlayerConnected")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnPlayerConnected));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

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

        var sender = serviceBusClientFactory.CreateSender("player_connected_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onPlayerConnected))).ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnChatMessage))]
    public async Task<HttpResponseData> OnChatMessage([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/OnChatMessage")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnChatMessage));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

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

        var sender = serviceBusClientFactory.CreateSender("chat_message_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onChatMessage))).ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnMapVote))]
    public async Task<HttpResponseData> OnMapVote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/OnMapVote")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnMapVote));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

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

        var sender = serviceBusClientFactory.CreateSender("map_vote_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onMapVote))).ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}