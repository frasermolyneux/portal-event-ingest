using System;

using MX.Api.Client.Configuration;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class EventIngestApiOptionsBuilder : ApiClientOptionsBuilder<EventIngestApiClientOptions, EventIngestApiOptionsBuilder>
{
    public EventIngestApiOptionsBuilder() : base() { }

    internal Action<CacheBuilder>? CapturedCacheConfigure { get; private set; }

    public new EventIngestApiOptionsBuilder WithCaching(Action<CacheBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        CapturedCacheConfigure = configure;
        return this;
    }
}
