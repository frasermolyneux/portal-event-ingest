using System.ComponentModel.DataAnnotations;
using System.Net;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.PlayerLifecycle;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V2;

public class PlayerLifecycleEvents
{
    private readonly IServiceBusClientFactory serviceBusClientFactory;

    public PlayerLifecycleEvents(IServiceBusClientFactory serviceBusClientFactory)
    {
        this.serviceBusClientFactory = serviceBusClientFactory;
    }

    [Function(nameof(ClientConnect))]
    public async Task<HttpResponseData> ClientConnect(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/player-lifecycle/connect")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientConnect));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientConnectEvent? clientConnectEvent;
        try
        {
            clientConnectEvent = JsonConvert.DeserializeObject<ClientConnectEvent>(requestBody);

            if (clientConnectEvent == null)
            {
                logger.LogWarning($"ClientConnect deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Validate required fields
            var validationContext = new ValidationContext(clientConnectEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientConnectEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientConnect validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientConnectEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientConnect;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientConnect Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientConnect was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_player_lifecycle_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientConnectEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientAuth))]
    public async Task<HttpResponseData> ClientAuth(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/player-lifecycle/auth")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientAuth));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientAuthEvent? clientAuthEvent;
        try
        {
            clientAuthEvent = JsonConvert.DeserializeObject<ClientAuthEvent>(requestBody);

            if (clientAuthEvent == null)
            {
                logger.LogWarning($"ClientAuth deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientAuthEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientAuthEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientAuth validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientAuthEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientAuth;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientAuth Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientAuth was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_player_lifecycle_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientAuthEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientJoin))]
    public async Task<HttpResponseData> ClientJoin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/player-lifecycle/join")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientJoin));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientJoinEvent? clientJoinEvent;
        try
        {
            clientJoinEvent = JsonConvert.DeserializeObject<ClientJoinEvent>(requestBody);

            if (clientJoinEvent == null)
            {
                logger.LogWarning($"ClientJoin deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientJoinEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientJoinEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientJoin validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientJoinEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientJoin;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientJoin Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientJoin was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_player_lifecycle_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientJoinEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientDisconnect))]
    public async Task<HttpResponseData> ClientDisconnect(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/player-lifecycle/disconnect")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientDisconnect));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientDisconnectEvent? clientDisconnectEvent;
        try
        {
            clientDisconnectEvent = JsonConvert.DeserializeObject<ClientDisconnectEvent>(requestBody);

            if (clientDisconnectEvent == null)
            {
                logger.LogWarning($"ClientDisconnect deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientDisconnectEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientDisconnectEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientDisconnect validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientDisconnectEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientDisconnect;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientDisconnect Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientDisconnect was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_player_lifecycle_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientDisconnectEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientNameChange))]
    public async Task<HttpResponseData> ClientNameChange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/player-lifecycle/name-change")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientNameChange));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientNameChangeEvent? clientNameChangeEvent;
        try
        {
            clientNameChangeEvent = JsonConvert.DeserializeObject<ClientNameChangeEvent>(requestBody);

            if (clientNameChangeEvent == null)
            {
                logger.LogWarning($"ClientNameChange deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientNameChangeEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientNameChangeEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientNameChange validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientNameChangeEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientNameChange;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientNameChange Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientNameChange was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_player_lifecycle_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientNameChangeEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
