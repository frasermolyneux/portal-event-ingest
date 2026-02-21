using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class VersionedApiHealthApi : IVersionedApiHealthApi
{
    public VersionedApiHealthApi(IApiHealthApi v1)
    {
        V1 = v1;
    }

    public IApiHealthApi V1 { get; }
}

public class VersionedApiInfoApi : IVersionedApiInfoApi
{
    public VersionedApiInfoApi(IApiInfoApi v1)
    {
        V1 = v1;
    }

    public IApiInfoApi V1 { get; }
}

public class VersionedPlayerEventsApi : IVersionedPlayerEventsApi
{
    public VersionedPlayerEventsApi(IPlayerEventsApi v1)
    {
        V1 = v1;
    }

    public IPlayerEventsApi V1 { get; }
}

public class VersionedServerEventsApi : IVersionedServerEventsApi
{
    public VersionedServerEventsApi(IServerEventsApi v1)
    {
        V1 = v1;
    }

    public IServerEventsApi V1 { get; }
}
