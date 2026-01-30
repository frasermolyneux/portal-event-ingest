using System.ComponentModel.DataAnnotations;
using System.Net;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Events.Abstractions.V2.Models.CustomPlugins;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V2;

public class MapVotingEvents
{
    private readonly IServiceBusClientFactory serviceBusClientFactory;

    public MapVotingEvents(IServiceBusClientFactory serviceBusClientFactory)
    {
        this.serviceBusClientFactory = serviceBusClientFactory;
    }

    [Function(nameof(ClientMapVoteLike))]
    public async Task<HttpResponseData> ClientMapVoteLike(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/map-voting/like")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientMapVoteLike));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientMapVoteLikeEvent? clientMapVoteLikeEvent;
        try
        {
            clientMapVoteLikeEvent = JsonConvert.DeserializeObject<ClientMapVoteLikeEvent>(requestBody);

            if (clientMapVoteLikeEvent == null)
            {
                logger.LogWarning($"ClientMapVoteLike deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientMapVoteLikeEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientMapVoteLikeEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientMapVoteLike validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientMapVoteLikeEvent.EventType = Abstractions.V2.Models.EventType.ClientMapVoteLike;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientMapVoteLike Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientMapVoteLike was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_map_voting_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientMapVoteLikeEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientMapVoteDislike))]
    public async Task<HttpResponseData> ClientMapVoteDislike(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/map-voting/dislike")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientMapVoteDislike));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientMapVoteDislikeEvent? clientMapVoteDislikeEvent;
        try
        {
            clientMapVoteDislikeEvent = JsonConvert.DeserializeObject<ClientMapVoteDislikeEvent>(requestBody);

            if (clientMapVoteDislikeEvent == null)
            {
                logger.LogWarning($"ClientMapVoteDislike deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientMapVoteDislikeEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientMapVoteDislikeEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientMapVoteDislike validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientMapVoteDislikeEvent.EventType = Abstractions.V2.Models.EventType.ClientMapVoteDislike;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientMapVoteDislike Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientMapVoteDislike was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_map_voting_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientMapVoteDislikeEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
