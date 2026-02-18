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
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.ChatMessages;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Moderation;

public class PlayerEventsIngestModerationTests
{
    private readonly Mock<ILogger<PlayerEventsIngest>> _mockLogger;
    private readonly Mock<IRepositoryApiClient> _mockRepoClient;
    private readonly IMemoryCache _memoryCache;
    private readonly TelemetryClient _telemetryClient;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ILocalWordListFilter> _mockWordFilter;
    private readonly Mock<IChatModerationService> _mockModerationService;
    private readonly Mock<IPlayersApi> _mockPlayersApi;
    private readonly Mock<IAdminActionsApi> _mockAdminActionsApi;
    private readonly Mock<IChatMessagesApi> _mockChatMessagesApi;
    private readonly IConfiguration _configuration;
    private readonly PlayerEventsIngest _sut;

    public PlayerEventsIngestModerationTests()
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
        _mockAdminActionsApi = new Mock<IAdminActionsApi>();
        _mockChatMessagesApi = new Mock<IChatMessagesApi>();

        var mockPlayersV1 = new Mock<IVersionedPlayersApi>();
        mockPlayersV1.Setup(p => p.V1).Returns(_mockPlayersApi.Object);
        _mockRepoClient.Setup(r => r.Players).Returns(mockPlayersV1.Object);

        var mockAdminActionsV1 = new Mock<IVersionedAdminActionsApi>();
        mockAdminActionsV1.Setup(a => a.V1).Returns(_mockAdminActionsApi.Object);
        _mockRepoClient.Setup(r => r.AdminActions).Returns(mockAdminActionsV1.Object);

        var mockChatMessagesV1 = new Mock<IVersionedChatMessagesApi>();
        mockChatMessagesV1.Setup(c => c.V1).Returns(_mockChatMessagesApi.Object);
        _mockRepoClient.Setup(r => r.ChatMessages).Returns(mockChatMessagesV1.Object);

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

    private static string CreateChatMessageJson(string message, string gameType = "CallOfDuty4", string username = "TestPlayer", string guid = "abc123")
    {
        var onChatMessage = new OnChatMessage
        {
            GameType = gameType,
            Username = username,
            Guid = guid,
            Message = message,
            Type = "All",
            ServerId = Guid.NewGuid(),
            EventGeneratedUtc = DateTime.UtcNow
        };
        return JsonConvert.SerializeObject(onChatMessage);
    }

    private void SetupPlayerExists(Guid playerId, DateTime firstSeen, bool hasModerateChatTag = false)
    {
        var playerDto = new PlayerDto();

        var type = typeof(PlayerDto);
        type.GetProperty(nameof(PlayerDto.PlayerId))!.SetValue(playerDto, playerId);
        type.GetProperty(nameof(PlayerDto.FirstSeen))!.SetValue(playerDto, firstSeen);
        type.GetProperty(nameof(PlayerDto.GameType))!.SetValue(playerDto, GameType.CallOfDuty4);

        if (hasModerateChatTag)
        {
            var tag = new TagDto { Name = "moderate-chat", UserDefined = true };
            var playerTag = new PlayerTagDto { Tag = tag };
            playerDto.Tags.Add(playerTag);
        }

        var apiResponse = new ApiResponse<PlayerDto>(playerDto);
        var result = new ApiResult<PlayerDto>(System.Net.HttpStatusCode.OK, apiResponse);

        _mockPlayersApi
            .Setup(p => p.GetPlayerByGameType(It.IsAny<GameType>(), It.IsAny<string>(), It.IsAny<PlayerEntityOptions>()))
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task ProcessOnChatMessage_WhenFeatureDisabled_DoesNotRunModeration()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(false);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        var json = CreateChatMessageJson("some test message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockWordFilter.Verify(w => w.Check(It.IsAny<string>()), Times.Never);
        _mockModerationService.Verify(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_WhenFeatureEnabled_AndWordFilterMatches_CreatesAdminAction()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>()))
            .Returns(new LocalFilterResult("Racial Slur", "badword"));
        _mockAdminActionsApi.Setup(a => a.CreateAdminAction(It.IsAny<CreateAdminActionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        var json = CreateChatMessageJson("some badword message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockAdminActionsApi.Verify(a => a.CreateAdminAction(
            It.Is<CreateAdminActionDto>(dto =>
                dto.PlayerId == playerId &&
                dto.Type == AdminActionType.Observation &&
                dto.Text.Contains("[Word Filter]") &&
                dto.Text.Contains("Racial Slur") &&
                dto.AdminId == "21145"),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockModerationService.Verify(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_WhenShortMessage_SkipsModeration()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        var json = CreateChatMessageJson("hi"); // 2 chars, below MinMessageLength of 5

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockWordFilter.Verify(w => w.Check(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_WhenQuickMessage_SkipsModeration()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        var json = CreateChatMessageJson("QUICKMESSAGE_hello_world");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockWordFilter.Verify(w => w.Check(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_NewPlayer_CallsContentSafetyApi()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-3)); // 3 days old, within 7-day window
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Returns((LocalFilterResult?)null);
        _mockModerationService.Setup(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatModerationResult(0, 0, 0, 0, 0, "Hate"));

        var json = CreateChatMessageJson("this is a normal chat message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockModerationService.Verify(m => m.AnalyseAsync("this is a normal chat message", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOnChatMessage_EstablishedPlayerWithoutTag_SkipsContentSafety()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-30)); // 30 days old, outside 7-day window
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Returns((LocalFilterResult?)null);

        var json = CreateChatMessageJson("this is a normal chat message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockModerationService.Verify(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_PlayerWithModerateTag_CallsContentSafety()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-30), hasModerateChatTag: true);
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Returns((LocalFilterResult?)null);
        _mockModerationService.Setup(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatModerationResult(0, 0, 0, 0, 0, "Hate"));

        var json = CreateChatMessageJson("this is a normal chat message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockModerationService.Verify(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOnChatMessage_ContentSafetyAboveThreshold_CreatesAdminAction()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Returns((LocalFilterResult?)null);
        _mockModerationService.Setup(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatModerationResult(6, 0, 0, 2, 6, "Hate"));
        _mockAdminActionsApi.Setup(a => a.CreateAdminAction(It.IsAny<CreateAdminActionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        var json = CreateChatMessageJson("some hateful message here");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockAdminActionsApi.Verify(a => a.CreateAdminAction(
            It.Is<CreateAdminActionDto>(dto =>
                dto.PlayerId == playerId &&
                dto.Type == AdminActionType.Observation &&
                dto.Text.Contains("[AI Content Safety]") &&
                dto.Text.Contains("Hate") &&
                dto.Text.Contains("severity 6/6") &&
                dto.AdminId == "21145"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessOnChatMessage_ContentSafetyBelowThreshold_DoesNotCreateAdminAction()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Returns((LocalFilterResult?)null);
        _mockModerationService.Setup(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatModerationResult(2, 0, 0, 0, 2, "Hate"));

        var json = CreateChatMessageJson("mildly rude message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockAdminActionsApi.Verify(a => a.CreateAdminAction(It.IsAny<CreateAdminActionDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_ContentSafetyReturnsNull_DoesNotCreateAdminAction()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>())).Returns((LocalFilterResult?)null);
        _mockModerationService.Setup(m => m.AnalyseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatModerationResult?)null);

        var json = CreateChatMessageJson("any message here");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockAdminActionsApi.Verify(a => a.CreateAdminAction(It.IsAny<CreateAdminActionDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOnChatMessage_AlwaysStoresChatMessage_RegardlessOfModeration()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupPlayerExists(playerId, DateTime.UtcNow.AddDays(-1));
        _mockFeatureManager.Setup(f => f.IsEnabledAsync("ChatToxicityDetection")).ReturnsAsync(true);
        _mockChatMessagesApi.Setup(c => c.CreateChatMessage(It.IsAny<CreateChatMessageDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));
        _mockWordFilter.Setup(w => w.Check(It.IsAny<string>()))
            .Returns(new LocalFilterResult("Racial Slur", "badword"));
        _mockAdminActionsApi.Setup(a => a.CreateAdminAction(It.IsAny<CreateAdminActionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult(System.Net.HttpStatusCode.OK));

        var json = CreateChatMessageJson("some badword message");

        // Act
        await _sut.ProcessOnChatMessage(json);

        // Assert
        _mockChatMessagesApi.Verify(c => c.CreateChatMessage(
            It.Is<CreateChatMessageDto>(dto => dto.PlayerId == playerId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
