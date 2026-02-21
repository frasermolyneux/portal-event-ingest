using System.Net;
using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;
using XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class FakeEventIngestApiClientTests
{
    [Fact]
    public async Task ApiHealth_DelegatesToHealthFake()
    {
        var client = new FakeEventIngestApiClient();
        client.HealthApi.WithStatusCode(HttpStatusCode.ServiceUnavailable);

        var result = await client.ApiHealth.V1.CheckHealth();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
    }

    [Fact]
    public async Task ApiInfo_DelegatesToInfoFake()
    {
        var client = new FakeEventIngestApiClient();
        client.InfoApi.WithInfo(EventIngestDtoFactory.CreateApiInfo(buildVersion: "2.0.0"));

        var result = await client.ApiInfo.V1.GetApiInfo();

        Assert.Equal("2.0.0", result.Result!.Data!.BuildVersion);
    }

    [Fact]
    public async Task PlayerEvents_TracksPlayerConnected()
    {
        var client = new FakeEventIngestApiClient();
        var evt = EventIngestDtoFactory.CreateOnPlayerConnected(username: "TestPlayer");

        var result = await client.PlayerEvents.V1.OnPlayerConnected(evt);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Single(client.PlayerEventsApiV1.PlayerConnectedEvents);
        Assert.Equal("TestPlayer", client.PlayerEventsApiV1.PlayerConnectedEvents.First().Username);
    }

    [Fact]
    public async Task PlayerEvents_TracksChatMessage()
    {
        var client = new FakeEventIngestApiClient();
        var evt = EventIngestDtoFactory.CreateOnChatMessage(message: "Hello");

        await client.PlayerEvents.V1.OnChatMessage(evt);

        Assert.Single(client.PlayerEventsApiV1.ChatMessageEvents);
        Assert.Equal("Hello", client.PlayerEventsApiV1.ChatMessageEvents.First().Message);
    }

    [Fact]
    public async Task PlayerEvents_TracksMapVote()
    {
        var client = new FakeEventIngestApiClient();
        var evt = EventIngestDtoFactory.CreateOnMapVote(mapName: "mp_crash", like: false);

        await client.PlayerEvents.V1.OnMapVote(evt);

        Assert.Single(client.PlayerEventsApiV1.MapVoteEvents);
        Assert.Equal("mp_crash", client.PlayerEventsApiV1.MapVoteEvents.First().MapName);
        Assert.False(client.PlayerEventsApiV1.MapVoteEvents.First().Like);
    }

    [Fact]
    public async Task ServerEvents_TracksServerConnected()
    {
        var client = new FakeEventIngestApiClient();
        var evt = EventIngestDtoFactory.CreateOnServerConnected(gameType: "CallOfDuty2");

        await client.ServerEvents.V1.OnServerConnected(evt);

        Assert.Single(client.ServerEventsApiV1.ServerConnectedEvents);
        Assert.Equal("CallOfDuty2", client.ServerEventsApiV1.ServerConnectedEvents.First().GameType);
    }

    [Fact]
    public async Task ServerEvents_TracksMapChange()
    {
        var client = new FakeEventIngestApiClient();
        var evt = EventIngestDtoFactory.CreateOnMapChange(mapName: "mp_backlot");

        await client.ServerEvents.V1.OnMapChange(evt);

        Assert.Single(client.ServerEventsApiV1.MapChangeEvents);
        Assert.Equal("mp_backlot", client.ServerEventsApiV1.MapChangeEvents.First().MapName);
    }

    [Fact]
    public void ImplementsIEventIngestApiClient()
    {
        IEventIngestApiClient client = new FakeEventIngestApiClient();

        Assert.NotNull(client.ApiHealth);
        Assert.NotNull(client.ApiHealth.V1);
        Assert.NotNull(client.ApiInfo);
        Assert.NotNull(client.ApiInfo.V1);
        Assert.NotNull(client.PlayerEvents);
        Assert.NotNull(client.PlayerEvents.V1);
        Assert.NotNull(client.ServerEvents);
        Assert.NotNull(client.ServerEvents.V1);
    }

    [Fact]
    public async Task Reset_ClearsAllFakeState()
    {
        var client = new FakeEventIngestApiClient();
        await client.PlayerEvents.V1.OnPlayerConnected(EventIngestDtoFactory.CreateOnPlayerConnected());
        await client.ServerEvents.V1.OnServerConnected(EventIngestDtoFactory.CreateOnServerConnected());

        client.Reset();

        Assert.Empty(client.PlayerEventsApiV1.PlayerConnectedEvents);
        Assert.Empty(client.ServerEventsApiV1.ServerConnectedEvents);
    }
}
