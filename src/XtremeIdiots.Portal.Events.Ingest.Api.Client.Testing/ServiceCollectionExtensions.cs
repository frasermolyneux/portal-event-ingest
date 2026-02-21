using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakeEventIngestApiClient(
        this IServiceCollection services,
        Action<FakeEventIngestApiClient>? configure = null)
    {
        var fakeClient = new FakeEventIngestApiClient();
        configure?.Invoke(fakeClient);

        services.RemoveAll<IEventIngestApiClient>();
        services.RemoveAll<IVersionedApiHealthApi>();
        services.RemoveAll<IVersionedApiInfoApi>();
        services.RemoveAll<IVersionedPlayerEventsApi>();
        services.RemoveAll<IVersionedServerEventsApi>();
        services.RemoveAll<IApiHealthApi>();
        services.RemoveAll<IApiInfoApi>();
        services.RemoveAll<IPlayerEventsApi>();
        services.RemoveAll<IServerEventsApi>();

        services.AddSingleton<IEventIngestApiClient>(fakeClient);
        services.AddSingleton<IVersionedApiHealthApi>(fakeClient.ApiHealth);
        services.AddSingleton<IVersionedApiInfoApi>(fakeClient.ApiInfo);
        services.AddSingleton<IVersionedPlayerEventsApi>(fakeClient.PlayerEvents);
        services.AddSingleton<IVersionedServerEventsApi>(fakeClient.ServerEvents);
        services.AddSingleton<IApiHealthApi>(fakeClient.HealthApi);
        services.AddSingleton<IApiInfoApi>(fakeClient.InfoApi);
        services.AddSingleton<IPlayerEventsApi>(fakeClient.PlayerEventsApiV1);
        services.AddSingleton<IServerEventsApi>(fakeClient.ServerEventsApiV1);

        return services;
    }
}
