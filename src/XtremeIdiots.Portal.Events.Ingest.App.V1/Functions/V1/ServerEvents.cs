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
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

        OnServerConnected? onServerConnected;
        try
        {
            onServerConnected = JsonConvert.DeserializeObject<OnServerConnected>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OnServerConnected was not in expected format. Raw input: {RawInput}", requestBody);
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid JSON format").ConfigureAwait(false);
            return badRequest;
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(onServerConnected?.Id) || !System.Guid.TryParse(onServerConnected.Id, out _))
            errors.Add("'Id' is required and must be a valid GUID");
        if (string.IsNullOrWhiteSpace(onServerConnected?.GameType))
            errors.Add("'GameType' is required");

        if (errors.Count > 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(string.Join("; ", errors)).ConfigureAwait(false);
            return badRequest;
        }

        await using var sender = serviceBusClientFactory.CreateSender("server_connected_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onServerConnected))).ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(OnMapChange))]
    public async Task<HttpResponseData> OnMapChange([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/OnMapChange")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(OnMapChange));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

        OnMapChange? onMapChange;
        try
        {
            onMapChange = JsonConvert.DeserializeObject<OnMapChange>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OnMapChange was not in expected format. Raw input: {RawInput}", requestBody);
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid JSON format").ConfigureAwait(false);
            return badRequest;
        }

        var errors = new List<string>();
        if (onMapChange == null || onMapChange.ServerId == System.Guid.Empty)
            errors.Add("'ServerId' is required and must not be empty");
        if (string.IsNullOrWhiteSpace(onMapChange?.GameName))
            errors.Add("'GameName' is required");
        if (string.IsNullOrWhiteSpace(onMapChange?.MapName))
            errors.Add("'MapName' is required");

        if (errors.Count > 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(string.Join("; ", errors)).ConfigureAwait(false);
            return badRequest;
        }

        await using var sender = serviceBusClientFactory.CreateSender("map_change_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onMapChange))).ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}