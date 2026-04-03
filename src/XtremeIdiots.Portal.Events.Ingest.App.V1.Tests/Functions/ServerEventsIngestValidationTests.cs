using Microsoft.Extensions.Logging;

using Moq;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Functions;

public class ServerEventsIngestValidationTests
{
    private readonly Mock<ILogger<ServerEventsIngest>> _mockLogger;
    private readonly Mock<IRepositoryApiClient> _mockRepoClient;
    private readonly Mock<IGameServersEventsApi> _mockGameServersEventsApi;
    private readonly ServerEventsIngest _sut;

    public ServerEventsIngestValidationTests()
    {
        _mockLogger = new Mock<ILogger<ServerEventsIngest>>();
        _mockRepoClient = new Mock<IRepositoryApiClient>();
        _mockGameServersEventsApi = new Mock<IGameServersEventsApi>();

        var mockGameServersEventsV1 = new Mock<IVersionedGameServersEventsApi>();
        mockGameServersEventsV1.Setup(g => g.V1).Returns(_mockGameServersEventsApi.Object);
        _mockRepoClient.Setup(r => r.GameServersEvents).Returns(mockGameServersEventsV1.Object);

        _sut = new ServerEventsIngest(_mockLogger.Object, _mockRepoClient.Object);
    }

    // --- ProcessOnServerConnected validation ---

    [Fact]
    public async Task ProcessOnServerConnected_InvalidJson_DiscardsMessage()
    {
        await _sut.ProcessOnServerConnected("not valid json{{{");
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnServerConnected_NullObject_DiscardsMessage()
    {
        await _sut.ProcessOnServerConnected("null");
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnServerConnected_InvalidId_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new OnServerConnected { Id = "not-a-guid", GameType = "CallOfDuty4" });
        await _sut.ProcessOnServerConnected(payload);
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnServerConnected_EmptyId_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new OnServerConnected { Id = "", GameType = "CallOfDuty4" });
        await _sut.ProcessOnServerConnected(payload);
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnServerConnected_ApiFailure_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnServerConnected { Id = Guid.NewGuid().ToString(), GameType = "CallOfDuty4" });
        _mockGameServersEventsApi.Setup(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.ProcessOnServerConnected(payload));
    }

    // --- ProcessOnMapChange validation ---

    [Fact]
    public async Task ProcessOnMapChange_InvalidJson_DiscardsMessage()
    {
        await _sut.ProcessOnMapChange("not valid json{{{");
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapChange_NullObject_DiscardsMessage()
    {
        await _sut.ProcessOnMapChange("null");
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapChange_EmptyServerId_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new OnMapChange { ServerId = Guid.Empty, GameType = "CallOfDuty4", GameName = "CoD4", MapName = "mp_crash", EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnMapChange(payload);
        _mockGameServersEventsApi.Verify(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapChange_ApiFailure_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnMapChange { ServerId = Guid.NewGuid(), GameType = "CallOfDuty4", GameName = "CoD4", MapName = "mp_crash", EventGeneratedUtc = DateTime.UtcNow });
        _mockGameServersEventsApi.Setup(g => g.CreateGameServerEvent(It.IsAny<CreateGameServerEventDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.ProcessOnMapChange(payload));
    }
}
