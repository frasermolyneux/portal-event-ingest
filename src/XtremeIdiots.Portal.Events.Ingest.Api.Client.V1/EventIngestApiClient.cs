namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class EventIngestApiClient : IEventIngestApiClient
{
    public EventIngestApiClient(
        IVersionedApiHealthApi apiHealth,
        IVersionedApiInfoApi apiInfo,
        IVersionedPlayerEventsApi playerEvents,
        IVersionedServerEventsApi serverEvents)
    {
        ApiHealth = apiHealth;
        ApiInfo = apiInfo;
        PlayerEvents = playerEvents;
        ServerEvents = serverEvents;
    }

    public IVersionedApiHealthApi ApiHealth { get; }
    public IVersionedApiInfoApi ApiInfo { get; }
    public IVersionedPlayerEventsApi PlayerEvents { get; }
    public IVersionedServerEventsApi ServerEvents { get; }
}
