using System.Net;
using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ReprocessDeadLetterQueue(ILogger<ReprocessDeadLetterQueue> logger, IServiceBusClientFactory serviceBusClientFactory)
{
    private const int BatchSize = 20;
    private const int DefaultMaxMessages = 50;
    private const int ThrottleDelayMs = 1000;
    private const int MaxBodyLogLength = 500;

    [Function(nameof(ReprocessDeadLetterQueue))]
    public async Task<HttpResponseData> RunReprocessDeadLetterQueue(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "api/v1/ReprocessDeadLetterQueue")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var queueName = req.Query["queueName"];
        if (string.IsNullOrEmpty(queueName))
        {
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest, new { error = "Please pass a queueName on the query string" }).ConfigureAwait(false);
        }

        if (!int.TryParse(req.Query["maxMessages"], out var maxMessages) || maxMessages <= 0)
        {
            maxMessages = DefaultMaxMessages;
        }

        var dryRun = string.Equals(req.Query["dryRun"], "true", StringComparison.OrdinalIgnoreCase);

        logger.LogInformation("ReprocessDeadLetterQueue started for queue '{QueueName}', maxMessages={MaxMessages}, dryRun={DryRun}",
            queueName, maxMessages, dryRun);

        try
        {
            int processed;

            if (dryRun)
            {
                processed = await PeekDeadLetterMessagesAsync(queueName, maxMessages).ConfigureAwait(false);

                return await CreateJsonResponse(req, HttpStatusCode.OK, new { queueName, peeked = processed, dryRun }).ConfigureAwait(false);
            }
            else
            {
                processed = await ReplayDeadLetterMessagesAsync(queueName, maxMessages).ConfigureAwait(false);

                return await CreateJsonResponse(req, HttpStatusCode.OK, new { queueName, replayed = processed, dryRun }).ConfigureAwait(false);
            }
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            logger.LogError(ex, "Queue '{QueueName}' not found", queueName);
            return await CreateJsonResponse(req, HttpStatusCode.NotFound, new { error = $"Queue '{queueName}' not found" }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing dead-letter messages for queue '{QueueName}'", queueName);
            return await CreateJsonResponse(req, HttpStatusCode.InternalServerError, new { error = "An internal error occurred while processing the request" }).ConfigureAwait(false);
        }
    }

    private async Task<int> ReplayDeadLetterMessagesAsync(string queueName, int maxMessages)
    {
        await using var sender = serviceBusClientFactory.CreateSender(queueName);
        await using var receiver = serviceBusClientFactory.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var totalProcessed = 0;

        while (totalProcessed < maxMessages)
        {
            var batchSize = Math.Min(BatchSize, maxMessages - totalProcessed);
            var dlqMessages = await receiver.ReceiveMessagesAsync(batchSize, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            if (dlqMessages.Count == 0)
                break;

            logger.LogInformation("Received {Count} dead-letter messages from '{QueueName}'", dlqMessages.Count, queueName);

            foreach (var dlqMessage in dlqMessages)
            {
                LogDeadLetterMessage(dlqMessage);

                var message = new ServiceBusMessage(dlqMessage);
                await sender.SendMessageAsync(message).ConfigureAwait(false);
                await receiver.CompleteMessageAsync(dlqMessage).ConfigureAwait(false);

                totalProcessed++;
            }

            if (totalProcessed < maxMessages)
            {
                await Task.Delay(ThrottleDelayMs).ConfigureAwait(false);
            }
        }

        logger.LogInformation("Replayed {TotalProcessed} dead-letter messages for queue '{QueueName}'", totalProcessed, queueName);
        return totalProcessed;
    }

    private async Task<int> PeekDeadLetterMessagesAsync(string queueName, int maxMessages)
    {
        await using var receiver = serviceBusClientFactory.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var totalPeeked = 0;
        long? fromSequenceNumber = null;

        while (totalPeeked < maxMessages)
        {
            var batchSize = Math.Min(BatchSize, maxMessages - totalPeeked);
            var dlqMessages = await receiver.PeekMessagesAsync(batchSize, fromSequenceNumber).ConfigureAwait(false);

            if (dlqMessages.Count == 0)
                break;

            logger.LogInformation("Peeked {Count} dead-letter messages from '{QueueName}'", dlqMessages.Count, queueName);

            foreach (var dlqMessage in dlqMessages)
            {
                LogDeadLetterMessage(dlqMessage);
                totalPeeked++;
                fromSequenceNumber = dlqMessage.SequenceNumber + 1;
            }

            if (totalPeeked < maxMessages)
            {
                await Task.Delay(ThrottleDelayMs).ConfigureAwait(false);
            }
        }

        logger.LogInformation("Peeked {TotalPeeked} dead-letter messages for queue '{QueueName}' (dry run)", totalPeeked, queueName);
        return totalPeeked;
    }

    private void LogDeadLetterMessage(ServiceBusReceivedMessage dlqMessage)
    {
        var body = dlqMessage.Body?.ToString() ?? string.Empty;
        var truncatedBody = body.Length > MaxBodyLogLength ? body[..MaxBodyLogLength] + "..." : body;

        logger.LogInformation(
            "DLQ message [{MessageId}]: Reason='{DeadLetterReason}', ErrorDescription='{DeadLetterErrorDescription}', Body='{TruncatedBody}'",
            dlqMessage.MessageId,
            dlqMessage.DeadLetterReason,
            dlqMessage.DeadLetterErrorDescription,
            truncatedBody);
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T payload)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await response.WriteStringAsync(json).ConfigureAwait(false);
        return response;
    }
}