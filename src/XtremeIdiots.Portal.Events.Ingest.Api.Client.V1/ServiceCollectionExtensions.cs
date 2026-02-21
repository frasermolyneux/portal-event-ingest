using Microsoft.Extensions.DependencyInjection;

using MX.Api.Client.Extensions;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventIngestApiClient(
        this IServiceCollection serviceCollection,
        Action<EventIngestApiOptionsBuilder> configureOptions)
    {
        // Register V1 API implementations
        serviceCollection.AddTypedApiClient<IApiHealthApi, ApiHealthApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(configureOptions);
        serviceCollection.AddTypedApiClient<IApiInfoApi, ApiInfoApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(configureOptions);
        serviceCollection.AddTypedApiClient<IPlayerEventsApi, PlayerEventsApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(configureOptions);
        serviceCollection.AddTypedApiClient<IServerEventsApi, ServerEventsApi, EventIngestApiClientOptions, EventIngestApiOptionsBuilder>(configureOptions);

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
