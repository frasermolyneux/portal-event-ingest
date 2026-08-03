using Microsoft.Extensions.DependencyInjection;

using MX.Api.Client.Configuration;
using MX.Api.Client.Extensions;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventIngestApiClient(
        this IServiceCollection serviceCollection,
        Action<EventIngestApiOptionsBuilder> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var probe = new EventIngestApiOptionsBuilder();
        configureOptions(probe);
        var capturedCache = probe.CapturedCacheConfigure;
        var sharedCache = capturedCache is null ? null : new SharedCacheConfiguration(capturedCache);
        Action<EventIngestApiOptionsBuilder> perClient = sharedCache is null
            ? configureOptions
            : builder => { configureOptions(builder); builder.WithSharedCaching(sharedCache); };

        // Register V1 API implementations
        serviceCollection.AddTypedApiClient<IApiHealthApi, ApiHealthApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(perClient);
        serviceCollection.AddTypedApiClient<IApiInfoApi, ApiInfoApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(perClient);
        serviceCollection.AddTypedApiClient<IPlayerEventsApi, PlayerEventsApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(perClient);
        serviceCollection.AddTypedApiClient<IServerEventsApi, ServerEventsApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(perClient);

        sharedCache?.ValidateAllOperationsMatched();

        // Register version selectors as scoped
        serviceCollection.AddScoped<IVersionedApiHealthApi, VersionedApiHealthApi>();
        serviceCollection.AddScoped<IVersionedApiInfoApi, VersionedApiInfoApi>();
        serviceCollection.AddScoped<IVersionedPlayerEventsApi, VersionedPlayerEventsApi>();
        serviceCollection.AddScoped<IVersionedServerEventsApi, VersionedServerEventsApi>();

        // Register the unified client as scoped
        serviceCollection.AddScoped<IEventIngestApiClient, EventIngestApiClient>();

        return serviceCollection;
    }
}
