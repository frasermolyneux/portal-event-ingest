namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public interface IEventIngestApiClient
{
    IVersionedApiHealthApi ApiHealth { get; }
    IVersionedApiInfoApi ApiInfo { get; }
    IVersionedPlayerEventsApi PlayerEvents { get; }
    IVersionedServerEventsApi ServerEvents { get; }
}
