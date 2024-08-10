using System.Net;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.EventIngestFunc;

public class ReprocessDeadLetterQueue
{
    private readonly ILogger<ReprocessDeadLetterQueue> logger;
    private readonly IConfiguration configuration;

    public ReprocessDeadLetterQueue(ILogger<ReprocessDeadLetterQueue> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    [Function(nameof(ReprocessDeadLetterQueue))]
    public async Task<HttpResponseData> RunReprocessDeadLetterQueue([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
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
            ServiceBusClient client = new ServiceBusClient(configuration["service_bus_connection_string"]);
            ServiceBusSender sender = client.CreateSender(queueName);

            ServiceBusReceiver receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
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

    private async Task ProcessDeadLetterMessagesAsync(ServiceBusSender sender, ServiceBusReceiver receiver)
    {
        int fetchCount = 150;

        IReadOnlyList<ServiceBusReceivedMessage> dlqMessages;
        do
        {
            dlqMessages = await receiver.ReceiveMessagesAsync(fetchCount);

            logger.LogInformation($"dl-count: {dlqMessages.Count}");

            int i = 1;

            foreach (var dlqMessage in dlqMessages)
            {
                ServiceBusMessage message = new(dlqMessage);

                await sender.SendMessageAsync(message);
                await receiver.CompleteMessageAsync(dlqMessage);

                i++;
            }
        } while (dlqMessages.Count > 0);

        await receiver.CloseAsync();
    }
}