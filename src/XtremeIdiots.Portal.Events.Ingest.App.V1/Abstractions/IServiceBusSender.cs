using Azure.Messaging.ServiceBus;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;

public interface IServiceBusSender
{
    Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default);
}
