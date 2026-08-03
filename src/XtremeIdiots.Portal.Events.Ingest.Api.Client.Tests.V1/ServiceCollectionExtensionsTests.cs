using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using MX.Api.Abstractions;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Tests.V1;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEventIngestApiClient_WithCrossSubApiCachingExpressions_ResolvesAllSubApis()
    {
        var services = new ServiceCollection();

        services.AddEventIngestApiClient(o => o
            .WithBaseUrl("https://events.example.com")
            .WithApiKeyAuthentication("test-api-key")
            .WithCachePartition("unit-tests")
            .WithCaching(c => c
                .NotCached<IPlayerEventsApi, Task<ApiResult>>(x => x.OnPlayerConnected(null!, CancellationToken.None))
                .NotCached<IServerEventsApi, Task<ApiResult>>(x => x.OnServerConnected(null!, CancellationToken.None))));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<IApiHealthApi>());
        Assert.NotNull(sp.GetRequiredService<IApiInfoApi>());
        Assert.NotNull(sp.GetRequiredService<IPlayerEventsApi>());
        Assert.NotNull(sp.GetRequiredService<IServerEventsApi>());
        Assert.NotNull(sp.GetRequiredService<IEventIngestApiClient>());
    }

    [Fact]
    public void AddEventIngestApiClient_WithCachingExpressionTargetingUnregisteredInterface_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
            services.AddEventIngestApiClient(o => o
                .WithBaseUrl("https://events.example.com")
                .WithApiKeyAuthentication("test-api-key")
                .WithCachePartition("unit-tests")
                .WithCaching(c => c
                    .NotCached<IBogusUnregisteredApi, Task<string>>(x => x.DoSomething(CancellationToken.None)))));
    }

    [Fact]
    public void AddEventIngestApiClient_WithoutCaching_ResolvesAllSubApis()
    {
        var services = new ServiceCollection();

        services.AddEventIngestApiClient(o => o
            .WithBaseUrl("https://events.example.com")
            .WithApiKeyAuthentication("test-api-key"));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<IApiHealthApi>());
        Assert.NotNull(sp.GetRequiredService<IApiInfoApi>());
        Assert.NotNull(sp.GetRequiredService<IPlayerEventsApi>());
        Assert.NotNull(sp.GetRequiredService<IServerEventsApi>());
        Assert.NotNull(sp.GetRequiredService<IEventIngestApiClient>());
    }

    public interface IBogusUnregisteredApi
    {
        Task<string> DoSomething(CancellationToken cancellationToken = default);
    }
}
