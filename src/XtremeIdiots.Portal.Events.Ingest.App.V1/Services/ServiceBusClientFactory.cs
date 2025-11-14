using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

public class ServiceBusClientFactory : IServiceBusClientFactory, IDisposable
{
    private readonly ServiceBusClient _client;

    public ServiceBusClientFactory(IConfiguration configuration)
    {
        var credential = new DefaultAzureCredential();
        var fullyQualifiedNamespace = configuration["ServiceBusConnection:fullyQualifiedNamespace"]
            ?? throw new InvalidOperationException("ServiceBusConnection:fullyQualifiedNamespace configuration is required");
        _client = new ServiceBusClient(fullyQualifiedNamespace, credential);
    }

    public IServiceBusSender CreateSender(string queueOrTopicName)
    {
        return new ServiceBusSenderWrapper(_client.CreateSender(queueOrTopicName));
    }

    public IServiceBusReceiver CreateReceiver(string queueName, ServiceBusReceiverOptions? options = null)
    {
        return new ServiceBusReceiverWrapper(_client.CreateReceiver(queueName, options));
    }

    public void Dispose()
    {
        _client?.DisposeAsync().AsTask().Wait();
    }
}
