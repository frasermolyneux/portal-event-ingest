using System.Net;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ServerEvents(IServiceBusClientFactory serviceBusClientFactory)
{
    [Function(nameof(OnServerConnected))]
    public async Task<HttpResponseData> OnServerConnected([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/OnServerConnected")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnServerConnected));
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

        var sender = serviceBusClientFactory.CreateSender("server_connected_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onServerConnected)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnMapChange))]
    public async Task<HttpResponseData> OnMapChange([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/OnMapChange")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnMapChange));
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

        var sender = serviceBusClientFactory.CreateSender("map_change_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onMapChange)));

        return req.CreateResponse(HttpStatusCode.OK);
    }
}