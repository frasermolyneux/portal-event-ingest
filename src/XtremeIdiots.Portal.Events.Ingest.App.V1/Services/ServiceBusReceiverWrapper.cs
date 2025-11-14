using Azure.Messaging.ServiceBus;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

public class ServiceBusReceiverWrapper : IServiceBusReceiver
{
    private readonly ServiceBusReceiver _receiver;

    public ServiceBusReceiverWrapper(ServiceBusReceiver receiver)
    {
        _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
    }

    public Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync(int maxMessages, TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = default)
    {
        return _receiver.ReceiveMessagesAsync(maxMessages, maxWaitTime, cancellationToken);
    }

    public Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
    {
        return _receiver.CompleteMessageAsync(message, cancellationToken);
    }

    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        return _receiver.CloseAsync(cancellationToken);
    }
}
