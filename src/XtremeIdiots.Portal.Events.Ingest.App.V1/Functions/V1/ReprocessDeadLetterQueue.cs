using System.Net;
using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ReprocessDeadLetterQueue(ILogger<ReprocessDeadLetterQueue> logger, IServiceBusClientFactory serviceBusClientFactory)
{
    [Function(nameof(ReprocessDeadLetterQueue))]
    public async Task<HttpResponseData> RunReprocessDeadLetterQueue([HttpTrigger(AuthorizationLevel.Function, "post", Route = "api/v1/ReprocessDeadLetterQueue")] HttpRequestData req, FunctionContext executionContext)
    {
        var queueName = req.Query["queueName"];
        if (string.IsNullOrEmpty(queueName))
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.WriteString("Please pass a queueName on the query string");
            return response;
        }

        try
        {
            var sender = serviceBusClientFactory.CreateSender(queueName);
            var receiver = serviceBusClientFactory.CreateReceiver(queueName, new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            await ProcessDeadLetterMessagesAsync(sender, receiver);
        }
        catch (ServiceBusException ex)
        {
            if (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                logger.LogError(ex, $"Queue '{queueName}' not found. Check that the name provided is correct.");
            }
            else
            {
                logger.LogError(ex, $"An error occurred while processing the request.");
            }
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }

    private async Task ProcessDeadLetterMessagesAsync(IServiceBusSender sender, IServiceBusReceiver receiver)
    {
        int fetchCount = 150;

        IReadOnlyList<ServiceBusReceivedMessage> dlqMessages;
        do
        {
            dlqMessages = await receiver.ReceiveMessagesAsync(fetchCount);

            logger.LogInformation($"dl-count: {dlqMessages.Count}");

            foreach (var dlqMessage in dlqMessages)
            {
                ServiceBusMessage message = new(dlqMessage);

                await sender.SendMessageAsync(message);
                await receiver.CompleteMessageAsync(dlqMessage);
            }
        } while (dlqMessages.Count > 0);

        await receiver.CloseAsync();
    }
}