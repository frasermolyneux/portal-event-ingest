using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

public class FakeVersionedApiHealthApi : IVersionedApiHealthApi
{
    public FakeVersionedApiHealthApi(FakeApiHealthApi v1) { V1 = v1; }
    public IApiHealthApi V1 { get; }
}

public class FakeVersionedApiInfoApi : IVersionedApiInfoApi
{
    public FakeVersionedApiInfoApi(FakeApiInfoApi v1) { V1 = v1; }
    public IApiInfoApi V1 { get; }
}

public class FakeVersionedPlayerEventsApi : IVersionedPlayerEventsApi
{
    public FakeVersionedPlayerEventsApi(FakePlayerEventsApi v1) { V1 = v1; }
    public IPlayerEventsApi V1 { get; }
}

public class FakeVersionedServerEventsApi : IVersionedServerEventsApi
{
    public FakeVersionedServerEventsApi(FakeServerEventsApi v1) { V1 = v1; }
    public IServerEventsApi V1 { get; }
}

public class FakeEventIngestApiClient : IEventIngestApiClient
{
    public FakeApiHealthApi HealthApi { get; } = new();
    public FakeApiInfoApi InfoApi { get; } = new();
    public FakePlayerEventsApi PlayerEventsApiV1 { get; } = new();
    public FakeServerEventsApi ServerEventsApiV1 { get; } = new();

    private readonly Lazy<FakeVersionedApiHealthApi> _apiHealth;
    private readonly Lazy<FakeVersionedApiInfoApi> _apiInfo;
    private readonly Lazy<FakeVersionedPlayerEventsApi> _playerEvents;
    private readonly Lazy<FakeVersionedServerEventsApi> _serverEvents;

    public FakeEventIngestApiClient()
    {
        _apiHealth = new Lazy<FakeVersionedApiHealthApi>(() => new FakeVersionedApiHealthApi(HealthApi));
        _apiInfo = new Lazy<FakeVersionedApiInfoApi>(() => new FakeVersionedApiInfoApi(InfoApi));
        _playerEvents = new Lazy<FakeVersionedPlayerEventsApi>(() => new FakeVersionedPlayerEventsApi(PlayerEventsApiV1));
        _serverEvents = new Lazy<FakeVersionedServerEventsApi>(() => new FakeVersionedServerEventsApi(ServerEventsApiV1));
    }

    public IVersionedApiHealthApi ApiHealth => _apiHealth.Value;
    public IVersionedApiInfoApi ApiInfo => _apiInfo.Value;
    public IVersionedPlayerEventsApi PlayerEvents => _playerEvents.Value;
    public IVersionedServerEventsApi ServerEvents => _serverEvents.Value;

    public FakeEventIngestApiClient Reset()
    {
        HealthApi.Reset();
        InfoApi.Reset();
        PlayerEventsApiV1.Reset();
        ServerEventsApiV1.Reset();
        return this;
    }
}
