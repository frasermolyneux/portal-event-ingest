using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

using Moq;

using MX.Api.Abstractions;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Abstractions.Models.V1;
using XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Functions;

public class PlayerEventsIngestValidationTests
{
    private readonly Mock<ILogger<PlayerEventsIngest>> _mockLogger;
    private readonly Mock<IRepositoryApiClient> _mockRepoClient;
    private readonly IMemoryCache _memoryCache;
    private readonly TelemetryClient _telemetryClient;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ILocalWordListFilter> _mockWordFilter;
    private readonly Mock<IChatModerationService> _mockModerationService;
    private readonly Mock<IPlayersApi> _mockPlayersApi;
    private readonly Mock<IChatMessagesApi> _mockChatMessagesApi;
    private readonly Mock<IAdminActionsApi> _mockAdminActionsApi;
    private readonly IConfiguration _configuration;
    private readonly PlayerEventsIngest _sut;

    public PlayerEventsIngestValidationTests()
    {
        _mockLogger = new Mock<ILogger<PlayerEventsIngest>>();
        _mockRepoClient = new Mock<IRepositoryApiClient>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        var mockChannel = new Mock<ITelemetryChannel>();
        mockChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()));
        _telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration { TelemetryChannel = mockChannel.Object });

        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockWordFilter = new Mock<ILocalWordListFilter>();
        _mockModerationService = new Mock<IChatModerationService>();

        _mockPlayersApi = new Mock<IPlayersApi>();
        _mockChatMessagesApi = new Mock<IChatMessagesApi>();
        _mockAdminActionsApi = new Mock<IAdminActionsApi>();

        var mockPlayersV1 = new Mock<IVersionedPlayersApi>();
        mockPlayersV1.Setup(p => p.V1).Returns(_mockPlayersApi.Object);
        _mockRepoClient.Setup(r => r.Players).Returns(mockPlayersV1.Object);

        var mockChatMessagesV1 = new Mock<IVersionedChatMessagesApi>();
        mockChatMessagesV1.Setup(c => c.V1).Returns(_mockChatMessagesApi.Object);
        _mockRepoClient.Setup(r => r.ChatMessages).Returns(mockChatMessagesV1.Object);

        var mockAdminActionsV1 = new Mock<IVersionedAdminActionsApi>();
        mockAdminActionsV1.Setup(a => a.V1).Returns(_mockAdminActionsApi.Object);
        _mockRepoClient.Setup(r => r.AdminActions).Returns(mockAdminActionsV1.Object);

        var configData = new Dictionary<string, string?>
        {
            ["ContentSafety:BotAdminId"] = "21145",
            ["ContentSafety:SeverityThreshold"] = "4",
            ["ContentSafety:NewPlayerWindowDays"] = "7",
            ["ContentSafety:MinMessageLength"] = "5",
            ["ContentSafety:ModerateChatTagName"] = "moderate-chat"
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        _sut = new PlayerEventsIngest(
            _mockLogger.Object,
            _mockRepoClient.Object,
            _memoryCache,
            _telemetryClient,
            _configuration,
            _mockFeatureManager.Object,
            _mockWordFilter.Object,
            _mockModerationService.Object);
    }

    // --- ProcessOnPlayerConnected validation ---

    [Fact]
    public async Task ProcessOnPlayerConnected_InvalidJson_DiscardsMessage()
    {
        await _sut.ProcessOnPlayerConnected("not valid json{{{");
        _mockPlayersApi.Verify(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_NullObject_DiscardsMessage()
    {
        await _sut.ProcessOnPlayerConnected("null");
        _mockPlayersApi.Verify(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_MissingGameType_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { Guid = "abc123", Username = "Test", IpAddress = "1.2.3.4", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnPlayerConnected(payload);
        _mockPlayersApi.Verify(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_MissingGuid_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { GameType = "CallOfDuty4", Username = "Test", IpAddress = "1.2.3.4", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnPlayerConnected(payload);
        _mockPlayersApi.Verify(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_InvalidGameType_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new OnPlayerConnected { GameType = "InvalidGame", Guid = "abc123", Username = "Test", IpAddress = "1.2.3.4", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnPlayerConnected(payload);
        _mockPlayersApi.Verify(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_ApiFailure_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnPlayerConnected { GameType = "CallOfDuty4", Guid = "abc123", Username = "Test", IpAddress = "1.2.3.4", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        _mockPlayersApi.Setup(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.ProcessOnPlayerConnected(payload));
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_CreatePlayerConflict_FallsThroughToUpdate()
    {
        var playerId = Guid.NewGuid();
        var payload = JsonConvert.SerializeObject(new OnPlayerConnected { GameType = "CallOfDuty4", Guid = "abc123", Username = "Test", IpAddress = "1.2.3.4", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });

        _mockPlayersApi.Setup(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.NotFound));
        _mockPlayersApi.Setup(p => p.CreatePlayer(It.IsAny<CreatePlayerDto>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.Conflict));

        var playerDto = new PlayerDto();
        typeof(PlayerDto).GetProperty(nameof(PlayerDto.PlayerId))!.SetValue(playerDto, playerId);
        typeof(PlayerDto).GetProperty(nameof(PlayerDto.GameType))!.SetValue(playerDto, GameType.CallOfDuty4);
        var apiResponse = new ApiResponse<PlayerDto>(playerDto);
        _mockPlayersApi.Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ReturnsAsync(new ApiResult<PlayerDto>(System.Net.HttpStatusCode.OK, apiResponse));
        _mockPlayersApi.Setup(p => p.UpdatePlayer(It.IsAny<EditPlayerDto>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        await _sut.ProcessOnPlayerConnected(payload);

        _mockPlayersApi.Verify(p => p.UpdatePlayer(It.IsAny<EditPlayerDto>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOnPlayerConnected_CreatePlayerFailure_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnPlayerConnected { GameType = "CallOfDuty4", Guid = "abc123", Username = "Test", IpAddress = "1.2.3.4", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });

        _mockPlayersApi.Setup(p => p.HeadPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.NotFound));
        _mockPlayersApi.Setup(p => p.CreatePlayer(It.IsAny<CreatePlayerDto>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessOnPlayerConnected(payload));
    }

    // --- ProcessOnChatMessage validation ---

    [Fact]
    public async Task ProcessOnChatMessage_InvalidJson_DiscardsMessage()
    {
        await _sut.ProcessOnChatMessage("not valid json{{{");
        _mockChatMessagesApi.Verify(c => c.CreateChatMessage(It.IsAny<Repository.Abstractions.Models.V1.ChatMessages.CreateChatMessageDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_NullObject_DiscardsMessage()
    {
        await _sut.ProcessOnChatMessage("null");
        _mockChatMessagesApi.Verify(c => c.CreateChatMessage(It.IsAny<Repository.Abstractions.Models.V1.ChatMessages.CreateChatMessageDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_MissingGameType_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { Guid = "abc123", Username = "Test", Message = "hello", Type = "All", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnChatMessage(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_MissingGuid_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { GameType = "CallOfDuty4", Username = "Test", Message = "hello", Type = "All", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnChatMessage(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_InvalidGameType_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new OnChatMessage { GameType = "InvalidGame", Guid = "abc123", Username = "Test", Message = "hello", Type = "All", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnChatMessage(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_ApiFailure_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnChatMessage { GameType = "CallOfDuty4", Guid = "abc123", Username = "Test", Message = "hello", Type = "All", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        _mockPlayersApi.Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.ProcessOnChatMessage(payload));
    }

    [Fact]
    public async Task ProcessOnChatMessage_PlayerNotFound_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnChatMessage { GameType = "CallOfDuty4", Guid = "abc123", Username = "Test", Message = "hello", Type = "All", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });

        _mockPlayersApi.Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ReturnsAsync(new ApiResult<PlayerDto>(System.Net.HttpStatusCode.NotFound, null!));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessOnChatMessage(payload));
    }

    [Fact]
    public async Task ProcessOnChatMessage_ModerationFailure_DoesNotThrow()
    {
        var playerId = Guid.NewGuid();
        var playerDto = new PlayerDto();
        typeof(PlayerDto).GetProperty(nameof(PlayerDto.PlayerId))!.SetValue(playerDto, playerId);
        typeof(PlayerDto).GetProperty(nameof(PlayerDto.FirstSeen))!.SetValue(playerDto, DateTime.UtcNow.AddDays(-1));
        typeof(PlayerDto).GetProperty(nameof(PlayerDto.GameType))!.SetValue(playerDto, GameType.CallOfDuty4);

        _mockPlayersApi.Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ReturnsAsync(new ApiResult<PlayerDto>(System.Net.HttpStatusCode.OK, new ApiResponse<PlayerDto>(playerDto)));
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<Repository.Abstractions.Models.V1.ChatMessages.CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("EventIngest.ChatToxicityDetection")).ReturnsAsync(true);
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Throws(new Exception("Moderation exploded"));

        var payload = JsonConvert.SerializeObject(new OnChatMessage { GameType = "CallOfDuty4", Guid = "abc123", Username = "Test", Message = "hello world", Type = "All", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });

        await _sut.ProcessOnChatMessage(payload);

        _mockChatMessagesApi.Verify(c => c.CreateChatMessage(It.IsAny<Repository.Abstractions.Models.V1.ChatMessages.CreateChatMessageDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- ProcessOnMapVote validation ---

    [Fact]
    public async Task ProcessOnMapVote_InvalidJson_DiscardsMessage()
    {
        await _sut.ProcessOnMapVote("not valid json{{{");
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapVote_NullObject_DiscardsMessage()
    {
        await _sut.ProcessOnMapVote("null");
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapVote_MissingGameType_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { Guid = "abc123", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnMapVote(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapVote_MissingGuid_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { GameType = "CallOfDuty4", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnMapVote(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapVote_MissingMapName_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new { GameType = "CallOfDuty4", Guid = "abc123", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnMapVote(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapVote_InvalidGameType_DiscardsMessage()
    {
        var payload = JsonConvert.SerializeObject(new OnMapVote { GameType = "InvalidGame", Guid = "abc123", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        await _sut.ProcessOnMapVote(payload);
        _mockPlayersApi.Verify(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnMapVote_ApiFailure_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnMapVote { GameType = "CallOfDuty4", Guid = "abc123", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });
        _mockPlayersApi.Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.ProcessOnMapVote(payload));
    }

    [Fact]
    public async Task ProcessOnMapVote_PlayerNotFound_ThrowsForRetry()
    {
        var payload = JsonConvert.SerializeObject(new OnMapVote { GameType = "CallOfDuty4", Guid = "abc123", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow });

        _mockPlayersApi.Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ReturnsAsync(new ApiResult<PlayerDto>(System.Net.HttpStatusCode.NotFound, null!));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessOnMapVote(payload));
    }
}
