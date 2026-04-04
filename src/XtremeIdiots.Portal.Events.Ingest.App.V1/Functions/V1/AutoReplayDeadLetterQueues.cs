using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class AutoReplayDeadLetterQueues(
    ILogger<AutoReplayDeadLetterQueues> logger,
    IServiceBusClientFactory serviceBusClientFactory,
    IFeatureManager featureManager)
{
    private const int BatchSize = 20;
    private const int MaxMessagesPerQueue = 100;
    private const int MaxBodyLogLength = 500;

    private static readonly string[] QueueNames =
    [
        "player_connected_queue",
        "chat_message_queue",
        "map_vote_queue",
        "server_connected_queue",
        "map_change_queue"
    ];

    [Function(nameof(AutoReplayDeadLetterQueues))]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
    {
        if (!await featureManager.IsEnabledAsync("AutoDlqReplay"))
        {
            logger.LogDebug("AutoDlqReplay feature flag is disabled, skipping");
            return;
        }

        logger.LogInformation("AutoReplayDeadLetterQueues started");

        var totalReplayed = 0;

        foreach (var queueName in QueueNames)
        {
            try
            {
                var replayed = await ReplayQueueDeadLettersAsync(queueName).ConfigureAwait(false);
                totalReplayed += replayed;

                if (replayed > 0)
                {
                    logger.LogInformation("AutoReplay: Replayed {Count} messages from '{QueueName}' DLQ", replayed, queueName);
                }
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                logger.LogWarning(ex, "AutoReplay: Queue '{QueueName}' not found, skipping", queueName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AutoReplay: Error processing DLQ for '{QueueName}', continuing with next queue", queueName);
            }
        }

        logger.LogInformation("AutoReplayDeadLetterQueues completed. Total replayed: {TotalReplayed}", totalReplayed);
    }

    private async Task<int> ReplayQueueDeadLettersAsync(string queueName)
    {
        await using var sender = serviceBusClientFactory.CreateSender(queueName);
        await using var receiver = serviceBusClientFactory.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var dlqMessages = await receiver.ReceiveMessagesAsync(BatchSize, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        if (dlqMessages.Count == 0)
            return 0;

        var replayed = 0;

        while (dlqMessages.Count > 0 && replayed < MaxMessagesPerQueue)
        {
            foreach (var dlqMessage in dlqMessages)
            {
                var body = dlqMessage.Body?.ToString() ?? string.Empty;
                var truncatedBody = body.Length > MaxBodyLogLength ? body[..MaxBodyLogLength] + "..." : body;

                logger.LogInformation(
                    "AutoReplay DLQ [{QueueName}] [{MessageId}]: Reason='{DeadLetterReason}', Body='{TruncatedBody}'",
                    queueName,
                    dlqMessage.MessageId,
                    dlqMessage.DeadLetterReason,
                    truncatedBody);

                var message = new ServiceBusMessage(dlqMessage);
                await sender.SendMessageAsync(message).ConfigureAwait(false);
                await receiver.CompleteMessageAsync(dlqMessage).ConfigureAwait(false);

                replayed++;

                if (replayed >= MaxMessagesPerQueue)
                    break;
            }

            if (replayed < MaxMessagesPerQueue)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                dlqMessages = await receiver.ReceiveMessagesAsync(BatchSize, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }

        return replayed;
    }
}
