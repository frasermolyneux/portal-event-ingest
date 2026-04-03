using System.Net;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

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
            logger.LogWarning(ex, "OnPlayerConnected was not in expected format. Raw input: {RawInput}", requestBody);
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid JSON format").ConfigureAwait(false);
            return badRequest;
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(onPlayerConnected?.GameType))
            errors.Add("'GameType' is required");
        if (string.IsNullOrWhiteSpace(onPlayerConnected?.Guid))
            errors.Add("'Guid' is required");
        if (onPlayerConnected?.GameType != null && !Enum.TryParse<GameType>(onPlayerConnected.GameType, out _))
            errors.Add($"Invalid 'GameType': {onPlayerConnected.GameType}");

        if (errors.Count > 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(string.Join("; ", errors)).ConfigureAwait(false);
            return badRequest;
        }

        await using var sender = serviceBusClientFactory.CreateSender("player_connected_queue");
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
            logger.LogWarning(ex, "OnChatMessage was not in expected format. Raw input: {RawInput}", requestBody);
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid JSON format").ConfigureAwait(false);
            return badRequest;
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(onChatMessage?.GameType))
            errors.Add("'GameType' is required");
        if (string.IsNullOrWhiteSpace(onChatMessage?.Guid))
            errors.Add("'Guid' is required");
        if (onChatMessage?.GameType != null && !Enum.TryParse<GameType>(onChatMessage.GameType, out _))
            errors.Add($"Invalid 'GameType': {onChatMessage.GameType}");

        if (errors.Count > 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(string.Join("; ", errors)).ConfigureAwait(false);
            return badRequest;
        }

        await using var sender = serviceBusClientFactory.CreateSender("chat_message_queue");
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
            logger.LogWarning(ex, "OnMapVote was not in expected format. Raw input: {RawInput}", requestBody);
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid JSON format").ConfigureAwait(false);
            return badRequest;
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(onMapVote?.GameType))
            errors.Add("'GameType' is required");
        if (string.IsNullOrWhiteSpace(onMapVote?.Guid))
            errors.Add("'Guid' is required");
        if (string.IsNullOrWhiteSpace(onMapVote?.MapName))
            errors.Add("'MapName' is required");
        if (onMapVote?.GameType != null && !Enum.TryParse<GameType>(onMapVote.GameType, out _))
            errors.Add($"Invalid 'GameType': {onMapVote.GameType}");

        if (errors.Count > 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(string.Join("; ", errors)).ConfigureAwait(false);
            return badRequest;
        }

        await using var sender = serviceBusClientFactory.CreateSender("map_vote_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(onMapVote))).ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}