using System.Net;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.EventIngestApi.Abstractions.Models;

namespace XtremeIdiots.Portal.EventIngestFunc;

public class ServerEvents
{
    private readonly IConfiguration configuration;

    public ServerEvents(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    [Function(nameof(OnServerConnected))]
    public async Task<HttpResponseData> OnServerConnected([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
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

        var credential = new DefaultAzureCredential();
        await using (var client = new ServiceBusClient(configuration["ServiceBusConnection:fullyQualifiedNamespace"], credential))
        {
            var sender = client.CreateSender("server_connected_queue");
            await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onServerConnected)));
        };

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnMapChange))]
    public async Task<HttpResponseData> OnMapChange([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
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

        var credential = new DefaultAzureCredential();
        await using (var client = new ServiceBusClient(configuration["ServiceBusConnection:fullyQualifiedNamespace"], credential))
        {
            var sender = client.CreateSender("map_change_queue");
            await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onMapChange)));
        };

        return req.CreateResponse(HttpStatusCode.OK);
    }
}