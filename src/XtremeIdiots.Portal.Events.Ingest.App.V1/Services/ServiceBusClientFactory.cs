using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

public sealed class ServiceBusClientFactory(IConfiguration configuration) : IServiceBusClientFactory, IAsyncDisposable, IDisposable
{
    private readonly ServiceBusClient _client = CreateClient(configuration);
    private int _disposed;

    private static ServiceBusClient CreateClient(IConfiguration configuration)
    {
        var managedIdentityClientId = configuration["ServiceBusConnection:ManagedIdentityClientId"]
            ?? configuration["AZURE_CLIENT_ID"];

        var credential = string.IsNullOrWhiteSpace(managedIdentityClientId)
            ? new DefaultAzureCredential()
            : new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = managedIdentityClientId
            });

        var fullyQualifiedNamespace = configuration["ServiceBusConnection:fullyQualifiedNamespace"]
            ?? throw new InvalidOperationException("ServiceBusConnection:fullyQualifiedNamespace configuration is required");
        return new ServiceBusClient(fullyQualifiedNamespace, credential);
    }

    public IServiceBusSender CreateSender(string queueOrTopicName)
        => new ServiceBusSenderWrapper(_client.CreateSender(queueOrTopicName));

    public IServiceBusReceiver CreateReceiver(string queueName, ServiceBusReceiverOptions? options = null)
        => new ServiceBusReceiverWrapper(_client.CreateReceiver(queueName, options));

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            await _client.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _client.DisposeAsync().AsTask().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }
    }
}
