using Azure.Messaging.ServiceBus;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

public interface IServiceBusClientFactory
{
    IServiceBusSender CreateSender(string queueOrTopicName);
    IServiceBusReceiver CreateReceiver(string queueName, ServiceBusReceiverOptions? options = null);
}
