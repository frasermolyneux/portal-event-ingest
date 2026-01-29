using System.ComponentModel.DataAnnotations;
using System.Net;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.ServerGame;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V2;

public class ServerGameEvents
{
    private readonly IServiceBusClientFactory serviceBusClientFactory;

    public ServerGameEvents(IServiceBusClientFactory serviceBusClientFactory)
    {
        this.serviceBusClientFactory = serviceBusClientFactory;
    }

    [Function(nameof(ServerStartup))]
    public async Task<HttpResponseData> ServerStartup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/startup")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ServerStartup));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ServerStartupEvent? serverStartupEvent;
        try
        {
            serverStartupEvent = JsonConvert.DeserializeObject<ServerStartupEvent>(requestBody);

            if (serverStartupEvent == null)
            {
                logger.LogWarning($"ServerStartup deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(serverStartupEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(serverStartupEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ServerStartup validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            serverStartupEvent.EventType = Abstractions.V2.Models.V2.EventType.ServerStartup;
        }
        catch (Exception ex)
        {
            logger.LogError($"ServerStartup Raw Input: '{requestBody}'");
            logger.LogError(ex, "ServerStartup was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(serverStartupEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(GameMapChange))]
    public async Task<HttpResponseData> GameMapChange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/map-change")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(GameMapChange));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GameMapChangeEvent? gameMapChangeEvent;
        try
        {
            gameMapChangeEvent = JsonConvert.DeserializeObject<GameMapChangeEvent>(requestBody);

            if (gameMapChangeEvent == null)
            {
                logger.LogWarning($"GameMapChange deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(gameMapChangeEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(gameMapChangeEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"GameMapChange validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            gameMapChangeEvent.EventType = Abstractions.V2.Models.V2.EventType.GameMapChange;
        }
        catch (Exception ex)
        {
            logger.LogError($"GameMapChange Raw Input: '{requestBody}'");
            logger.LogError(ex, "GameMapChange was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(gameMapChangeEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(GameRoundStart))]
    public async Task<HttpResponseData> GameRoundStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/round-start")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(GameRoundStart));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GameRoundStartEvent? gameRoundStartEvent;
        try
        {
            gameRoundStartEvent = JsonConvert.DeserializeObject<GameRoundStartEvent>(requestBody);

            if (gameRoundStartEvent == null)
            {
                logger.LogWarning($"GameRoundStart deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(gameRoundStartEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(gameRoundStartEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"GameRoundStart validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            gameRoundStartEvent.EventType = Abstractions.V2.Models.V2.EventType.GameRoundStart;
        }
        catch (Exception ex)
        {
            logger.LogError($"GameRoundStart Raw Input: '{requestBody}'");
            logger.LogError(ex, "GameRoundStart was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(gameRoundStartEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(GameRoundEnd))]
    public async Task<HttpResponseData> GameRoundEnd(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/round-end")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(GameRoundEnd));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GameRoundEndEvent? gameRoundEndEvent;
        try
        {
            gameRoundEndEvent = JsonConvert.DeserializeObject<GameRoundEndEvent>(requestBody);

            if (gameRoundEndEvent == null)
            {
                logger.LogWarning($"GameRoundEnd deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(gameRoundEndEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(gameRoundEndEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"GameRoundEnd validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            gameRoundEndEvent.EventType = Abstractions.V2.Models.V2.EventType.GameRoundEnd;
        }
        catch (Exception ex)
        {
            logger.LogError($"GameRoundEnd Raw Input: '{requestBody}'");
            logger.LogError(ex, "GameRoundEnd was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(gameRoundEndEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(GameExit))]
    public async Task<HttpResponseData> GameExit(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/exit")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(GameExit));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GameExitEvent? gameExitEvent;
        try
        {
            gameExitEvent = JsonConvert.DeserializeObject<GameExitEvent>(requestBody);

            if (gameExitEvent == null)
            {
                logger.LogWarning($"GameExit deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(gameExitEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(gameExitEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"GameExit validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            gameExitEvent.EventType = Abstractions.V2.Models.V2.EventType.GameExit;
        }
        catch (Exception ex)
        {
            logger.LogError($"GameExit Raw Input: '{requestBody}'");
            logger.LogError(ex, "GameExit was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(gameExitEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(GameWarmup))]
    public async Task<HttpResponseData> GameWarmup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/warmup")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(GameWarmup));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GameWarmupEvent? gameWarmupEvent;
        try
        {
            gameWarmupEvent = JsonConvert.DeserializeObject<GameWarmupEvent>(requestBody);

            if (gameWarmupEvent == null)
            {
                logger.LogWarning($"GameWarmup deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(gameWarmupEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(gameWarmupEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"GameWarmup validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            gameWarmupEvent.EventType = Abstractions.V2.Models.V2.EventType.GameWarmup;
        }
        catch (Exception ex)
        {
            logger.LogError($"GameWarmup Raw Input: '{requestBody}'");
            logger.LogError(ex, "GameWarmup was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(gameWarmupEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(GameRoundPlayerScores))]
    public async Task<HttpResponseData> GameRoundPlayerScores(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/server-game/player-scores")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(GameRoundPlayerScores));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        GameRoundPlayerScoresEvent? gameRoundPlayerScoresEvent;
        try
        {
            gameRoundPlayerScoresEvent = JsonConvert.DeserializeObject<GameRoundPlayerScoresEvent>(requestBody);

            if (gameRoundPlayerScoresEvent == null)
            {
                logger.LogWarning($"GameRoundPlayerScores deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(gameRoundPlayerScoresEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(gameRoundPlayerScoresEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"GameRoundPlayerScores validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            gameRoundPlayerScoresEvent.EventType = Abstractions.V2.Models.V2.EventType.GameRoundPlayerScores;
        }
        catch (Exception ex)
        {
            logger.LogError($"GameRoundPlayerScores Raw Input: '{requestBody}'");
            logger.LogError(ex, "GameRoundPlayerScores was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_server_game_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(gameRoundPlayerScoresEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
