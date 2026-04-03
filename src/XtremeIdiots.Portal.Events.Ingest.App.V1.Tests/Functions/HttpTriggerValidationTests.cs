using System.Net;
using System.Text;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Moq;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Helpers;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Functions;

public class HttpTriggerValidationTests
{
    private readonly Mock<IServiceBusClientFactory> _mockFactory;
    private readonly Mock<IServiceBusSender> _mockSender;
    private readonly Mock<FunctionContext> _mockFunctionContext;

    public HttpTriggerValidationTests()
    {
        _mockFactory = new Mock<IServiceBusClientFactory>();
        _mockSender = new Mock<IServiceBusSender>();

        _mockFactory.Setup(f => f.CreateSender(It.IsAny<string>())).Returns(_mockSender.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

        _mockFunctionContext = new Mock<FunctionContext>();
        _mockFunctionContext.Setup(c => c.InstanceServices).Returns(mockServiceProvider.Object);
    }

    #region OnPlayerConnected

    [Fact]
    public async Task OnPlayerConnected_ValidPayload_Returns200AndSendsMessage()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { GameType = "CallOfDuty4", Guid = "abc123", Username = "player1", IpAddress = "127.0.0.1", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnPlayerConnected", payload);

        // Act
        var response = await sut.OnPlayerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnPlayerConnected_InvalidJson_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var request = CreateRequestWithBody("http://localhost/v1/OnPlayerConnected", "not valid json {{{");

        // Act
        var response = await sut.OnPlayerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid JSON format", body);
    }

    [Fact]
    public async Task OnPlayerConnected_MissingRequiredFields_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { Username = "player1" }; // missing GameType and Guid
        var request = CreateRequest("http://localhost/v1/OnPlayerConnected", payload);

        // Act
        var response = await sut.OnPlayerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'GameType' is required", body);
        Assert.Contains("'Guid' is required", body);
    }

    [Fact]
    public async Task OnPlayerConnected_InvalidGameType_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { GameType = "InvalidGame", Guid = "abc123", Username = "player1", IpAddress = "127.0.0.1", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnPlayerConnected", payload);

        // Act
        var response = await sut.OnPlayerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid 'GameType'", body);
    }

    #endregion

    #region OnChatMessage

    [Fact]
    public async Task OnChatMessage_ValidPayload_Returns200AndSendsMessage()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { GameType = "CallOfDuty4", Guid = "abc123", Username = "player1", Message = "hello", Type = "say", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnChatMessage", payload);

        // Act
        var response = await sut.OnChatMessage(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnChatMessage_InvalidJson_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var request = CreateRequestWithBody("http://localhost/v1/OnChatMessage", "{bad json");

        // Act
        var response = await sut.OnChatMessage(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid JSON format", body);
    }

    [Fact]
    public async Task OnChatMessage_MissingRequiredFields_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { Message = "hello" }; // missing GameType and Guid
        var request = CreateRequest("http://localhost/v1/OnChatMessage", payload);

        // Act
        var response = await sut.OnChatMessage(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'GameType' is required", body);
        Assert.Contains("'Guid' is required", body);
    }

    [Fact]
    public async Task OnChatMessage_InvalidGameType_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { GameType = "NotAGame", Guid = "abc123", Username = "player1", Message = "hi", Type = "say", ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnChatMessage", payload);

        // Act
        var response = await sut.OnChatMessage(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid 'GameType'", body);
    }

    #endregion

    #region OnMapVote

    [Fact]
    public async Task OnMapVote_ValidPayload_Returns200AndSendsMessage()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { GameType = "CallOfDuty4", Guid = "abc123", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnMapVote", payload);

        // Act
        var response = await sut.OnMapVote(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnMapVote_InvalidJson_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var request = CreateRequestWithBody("http://localhost/v1/OnMapVote", "{invalid json!!");

        // Act
        var response = await sut.OnMapVote(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid JSON format", body);
    }

    [Fact]
    public async Task OnMapVote_MissingRequiredFields_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { Like = true }; // missing GameType, Guid, MapName
        var request = CreateRequest("http://localhost/v1/OnMapVote", payload);

        // Act
        var response = await sut.OnMapVote(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'GameType' is required", body);
        Assert.Contains("'Guid' is required", body);
        Assert.Contains("'MapName' is required", body);
    }

    [Fact]
    public async Task OnMapVote_InvalidGameType_Returns400()
    {
        // Arrange
        var sut = new PlayerEvents(_mockFactory.Object);
        var payload = new { GameType = "FakeGame", Guid = "abc123", MapName = "mp_crash", Like = true, ServerId = Guid.NewGuid(), EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnMapVote", payload);

        // Act
        var response = await sut.OnMapVote(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid 'GameType'", body);
    }

    #endregion

    #region OnServerConnected

    [Fact]
    public async Task OnServerConnected_ValidPayload_Returns200AndSendsMessage()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var payload = new { Id = Guid.NewGuid().ToString(), GameType = "CallOfDuty4" };
        var request = CreateRequest("http://localhost/v1/OnServerConnected", payload);

        // Act
        var response = await sut.OnServerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnServerConnected_InvalidJson_Returns400()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var request = CreateRequestWithBody("http://localhost/v1/OnServerConnected", "{{bad}}");

        // Act
        var response = await sut.OnServerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid JSON format", body);
    }

    [Fact]
    public async Task OnServerConnected_MissingRequiredFields_Returns400()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var payload = new { Hostname = "test-server" }; // missing Id and GameType
        var request = CreateRequest("http://localhost/v1/OnServerConnected", payload);

        // Act
        var response = await sut.OnServerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'Id' is required", body);
        Assert.Contains("'GameType' is required", body);
    }

    [Fact]
    public async Task OnServerConnected_InvalidIdNotGuid_Returns400()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var payload = new { Id = "not-a-guid", GameType = "CallOfDuty4" };
        var request = CreateRequest("http://localhost/v1/OnServerConnected", payload);

        // Act
        var response = await sut.OnServerConnected(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'Id' is required and must be a valid GUID", body);
    }

    #endregion

    #region OnMapChange

    [Fact]
    public async Task OnMapChange_ValidPayload_Returns200AndSendsMessage()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var payload = new { ServerId = Guid.NewGuid(), GameName = "cod4", MapName = "mp_crash", GameType = "CallOfDuty4", EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnMapChange", payload);

        // Act
        var response = await sut.OnMapChange(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnMapChange_InvalidJson_Returns400()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var request = CreateRequestWithBody("http://localhost/v1/OnMapChange", "not-json");

        // Act
        var response = await sut.OnMapChange(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Invalid JSON format", body);
    }

    [Fact]
    public async Task OnMapChange_MissingRequiredFields_Returns400()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var payload = new { GameType = "CallOfDuty4", EventGeneratedUtc = DateTime.UtcNow }; // missing ServerId, GameName, MapName
        var request = CreateRequest("http://localhost/v1/OnMapChange", payload);

        // Act
        var response = await sut.OnMapChange(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'ServerId' is required", body);
        Assert.Contains("'GameName' is required", body);
        Assert.Contains("'MapName' is required", body);
    }

    [Fact]
    public async Task OnMapChange_EmptyServerId_Returns400()
    {
        // Arrange
        var sut = new ServerEvents(_mockFactory.Object);
        var payload = new { ServerId = Guid.Empty, GameName = "cod4", MapName = "mp_crash", GameType = "CallOfDuty4", EventGeneratedUtc = DateTime.UtcNow };
        var request = CreateRequest("http://localhost/v1/OnMapChange", payload);

        // Act
        var response = await sut.OnMapChange(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("'ServerId' is required", body);
    }

    #endregion

    #region Helpers

    private FakeHttpRequestData CreateRequest(string url, object payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        return CreateRequestWithBody(url, json);
    }

    private FakeHttpRequestData CreateRequestWithBody(string url, string body)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return new FakeHttpRequestData(_mockFunctionContext.Object, new Uri(url), stream);
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    #endregion
}
