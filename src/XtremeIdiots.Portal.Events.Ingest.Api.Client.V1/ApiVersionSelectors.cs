using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public interface IVersionedApiHealthApi
{
    IApiHealthApi V1 { get; }
}

public interface IVersionedApiInfoApi
{
    IApiInfoApi V1 { get; }
}

public interface IVersionedPlayerEventsApi
{
    IPlayerEventsApi V1 { get; }
}

public interface IVersionedServerEventsApi
{
    IServerEventsApi V1 { get; }
}
