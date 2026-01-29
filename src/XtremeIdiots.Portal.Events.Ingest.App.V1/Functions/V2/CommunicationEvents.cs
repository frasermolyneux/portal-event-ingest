using System.ComponentModel.DataAnnotations;
using System.Net;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Events.Abstractions.V2.Models.V2.Communication;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V2;

public class CommunicationEvents
{
    private readonly IServiceBusClientFactory serviceBusClientFactory;

    public CommunicationEvents(IServiceBusClientFactory serviceBusClientFactory)
    {
        this.serviceBusClientFactory = serviceBusClientFactory;
    }

    [Function(nameof(ClientSay))]
    public async Task<HttpResponseData> ClientSay(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/communication/say")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientSay));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientSayEvent? clientSayEvent;
        try
        {
            clientSayEvent = JsonConvert.DeserializeObject<ClientSayEvent>(requestBody);

            if (clientSayEvent == null)
            {
                logger.LogWarning($"ClientSay deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientSayEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientSayEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientSay validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientSayEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientSay;
            clientSayEvent.CommunicationType = Abstractions.V2.Models.V2.CommunicationType.Public;
            clientSayEvent.MessageLength = clientSayEvent.Message?.Length ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientSay Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientSay was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_communication_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientSayEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientTeamSay))]
    public async Task<HttpResponseData> ClientTeamSay(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/communication/team-say")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientTeamSay));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientTeamSayEvent? clientTeamSayEvent;
        try
        {
            clientTeamSayEvent = JsonConvert.DeserializeObject<ClientTeamSayEvent>(requestBody);

            if (clientTeamSayEvent == null)
            {
                logger.LogWarning($"ClientTeamSay deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientTeamSayEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientTeamSayEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientTeamSay validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientTeamSayEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientTeamSay;
            clientTeamSayEvent.CommunicationType = Abstractions.V2.Models.V2.CommunicationType.Team;
            clientTeamSayEvent.MessageLength = clientTeamSayEvent.Message?.Length ?? 0;
            clientTeamSayEvent.TargetTeam = clientTeamSayEvent.PlayerTeam;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientTeamSay Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientTeamSay was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_communication_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientTeamSayEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientSquadSay))]
    public async Task<HttpResponseData> ClientSquadSay(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/communication/squad-say")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientSquadSay));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientSquadSayEvent? clientSquadSayEvent;
        try
        {
            clientSquadSayEvent = JsonConvert.DeserializeObject<ClientSquadSayEvent>(requestBody);

            if (clientSquadSayEvent == null)
            {
                logger.LogWarning($"ClientSquadSay deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientSquadSayEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientSquadSayEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientSquadSay validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientSquadSayEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientSquadSay;
            clientSquadSayEvent.CommunicationType = Abstractions.V2.Models.V2.CommunicationType.Squad;
            clientSquadSayEvent.MessageLength = clientSquadSayEvent.Message?.Length ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientSquadSay Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientSquadSay was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_communication_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientSquadSayEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientPrivateSay))]
    public async Task<HttpResponseData> ClientPrivateSay(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/communication/private-say")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientPrivateSay));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientPrivateSayEvent? clientPrivateSayEvent;
        try
        {
            clientPrivateSayEvent = JsonConvert.DeserializeObject<ClientPrivateSayEvent>(requestBody);

            if (clientPrivateSayEvent == null)
            {
                logger.LogWarning($"ClientPrivateSay deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientPrivateSayEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientPrivateSayEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientPrivateSay validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientPrivateSayEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientPrivateSay;
            clientPrivateSayEvent.CommunicationType = Abstractions.V2.Models.V2.CommunicationType.Private;
            clientPrivateSayEvent.MessageLength = clientPrivateSayEvent.Message?.Length ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientPrivateSay Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientPrivateSay was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_communication_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientPrivateSayEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function(nameof(ClientRadio))]
    public async Task<HttpResponseData> ClientRadio(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/communication/radio")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(ClientRadio));
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        ClientRadioEvent? clientRadioEvent;
        try
        {
            clientRadioEvent = JsonConvert.DeserializeObject<ClientRadioEvent>(requestBody);

            if (clientRadioEvent == null)
            {
                logger.LogWarning($"ClientRadio deserialized to null. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var validationContext = new ValidationContext(clientRadioEvent);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(clientRadioEvent, validationContext, validationResults, true))
            {
                logger.LogWarning($"ClientRadio validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}. Raw Input: '{requestBody}'");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            clientRadioEvent.EventType = Abstractions.V2.Models.V2.EventType.ClientRadio;
            clientRadioEvent.CommunicationType = Abstractions.V2.Models.V2.CommunicationType.Radio;
            clientRadioEvent.MessageLength = clientRadioEvent.Message?.Length ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClientRadio Raw Input: '{requestBody}'");
            logger.LogError(ex, "ClientRadio was not in expected format");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sender = serviceBusClientFactory.CreateSender("v2_communication_queue");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(clientRadioEvent)));

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
