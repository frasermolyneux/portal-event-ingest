using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFakeEventIngestApiClient_RegistersAllServices()
    {
        var services = new ServiceCollection();

        services.AddFakeEventIngestApiClient();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IEventIngestApiClient>());
        Assert.NotNull(provider.GetRequiredService<IVersionedApiHealthApi>());
        Assert.NotNull(provider.GetRequiredService<IVersionedApiInfoApi>());
        Assert.NotNull(provider.GetRequiredService<IVersionedPlayerEventsApi>());
        Assert.NotNull(provider.GetRequiredService<IVersionedServerEventsApi>());
        Assert.NotNull(provider.GetRequiredService<IApiHealthApi>());
        Assert.NotNull(provider.GetRequiredService<IApiInfoApi>());
        Assert.NotNull(provider.GetRequiredService<IPlayerEventsApi>());
        Assert.NotNull(provider.GetRequiredService<IServerEventsApi>());
    }

    [Fact]
    public async Task AddFakeEventIngestApiClient_ConfiguredResponses_ResolveCorrectly()
    {
        var services = new ServiceCollection();

        services.AddFakeEventIngestApiClient(client =>
        {
            client.InfoApi.WithInfo(EventIngestDtoFactory.CreateApiInfo(buildVersion: "3.0.0"));
        });

        var provider = services.BuildServiceProvider();
        var apiClient = provider.GetRequiredService<IEventIngestApiClient>();
        var result = await apiClient.ApiInfo.V1.GetApiInfo();

        Assert.Equal("3.0.0", result.Result!.Data!.BuildVersion);
    }

    [Fact]
    public void AddFakeEventIngestApiClient_WithoutConfigure_RegistersWithDefaults()
    {
        var services = new ServiceCollection();

        services.AddFakeEventIngestApiClient();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IEventIngestApiClient>());
    }
}
