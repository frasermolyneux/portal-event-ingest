using Azure.Messaging.ServiceBus;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

public interface IServiceBusReceiver : IAsyncDisposable
{
    Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync(int maxMessages, TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = default);
    Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default);
    Task CloseAsync(CancellationToken cancellationToken = default);
}
