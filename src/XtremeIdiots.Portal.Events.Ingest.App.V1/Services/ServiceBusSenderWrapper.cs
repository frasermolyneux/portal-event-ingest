using Azure.Messaging.ServiceBus;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

public class ServiceBusSenderWrapper : IServiceBusSender
{
    private readonly ServiceBusSender _sender;

    public ServiceBusSenderWrapper(ServiceBusSender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
    {
        return _sender.SendMessageAsync(message, cancellationToken);
    }
}
