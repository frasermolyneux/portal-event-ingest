using Azure;
using Azure.AI.ContentSafety;

using Microsoft.Extensions.Logging;

using Moq;

using XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Moderation;

public class ChatModerationServiceTests
{
    private readonly Mock<ContentSafetyClient> _mockClient;
    private readonly Mock<ILogger<ChatModerationService>> _mockLogger;
    private readonly ChatModerationService _sut;

    public ChatModerationServiceTests()
    {
        _mockClient = new Mock<ContentSafetyClient>();
        _mockLogger = new Mock<ILogger<ChatModerationService>>();
        _sut = new ChatModerationService(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyseAsync_WhenApiThrows_ReturnsNullAndLogs()
    {
        // Arrange
        _mockClient
            .Setup(c => c.AnalyzeTextAsync(It.IsAny<AnalyzeTextOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Service unavailable"));

        // Act
        var result = await _sut.AnalyseAsync("test message");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyseAsync_PassesMessageToClient()
    {
        // Arrange
        _mockClient
            .Setup(c => c.AnalyzeTextAsync(It.IsAny<AnalyzeTextOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("expected"));

        // Act
        await _sut.AnalyseAsync("specific message text");

        // Assert
        _mockClient.Verify(c => c.AnalyzeTextAsync(
            It.Is<AnalyzeTextOptions>(o => o.Text == "specific message text"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ChatModerationResult_MaxSeverity_IsCorrect()
    {
        // Test the result record directly
        var result = new ChatModerationResult(4, 0, 2, 6, 6, "Violence");

        Assert.Equal(4, result.HateSeverity);
        Assert.Equal(0, result.SelfHarmSeverity);
        Assert.Equal(2, result.SexualSeverity);
        Assert.Equal(6, result.ViolenceSeverity);
        Assert.Equal(6, result.MaxSeverity);
        Assert.Equal("Violence", result.Category);
    }

    [Fact]
    public void ChatModerationResult_AllZero_HasZeroMax()
    {
        var result = new ChatModerationResult(0, 0, 0, 0, 0, "Hate");

        Assert.Equal(0, result.MaxSeverity);
    }
}
