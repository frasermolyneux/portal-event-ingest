using Azure.Messaging.ServiceBus;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

public class ServiceBusSenderWrapper(ServiceBusSender sender) : IServiceBusSender
{
    private readonly ServiceBusSender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    public Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
        => _sender.SendMessageAsync(message, cancellationToken);
}
