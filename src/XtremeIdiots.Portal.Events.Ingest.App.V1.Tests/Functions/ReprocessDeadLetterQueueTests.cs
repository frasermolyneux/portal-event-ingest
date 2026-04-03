using System.Net;
using System.Text;
using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Moq;

using XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Helpers;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Functions;

public class ReprocessDeadLetterQueueTests
{
    private readonly Mock<ILogger<ReprocessDeadLetterQueue>> _mockLogger;
    private readonly Mock<IServiceBusClientFactory> _mockFactory;
    private readonly Mock<IServiceBusSender> _mockSender;
    private readonly Mock<IServiceBusReceiver> _mockReceiver;
    private readonly Mock<FunctionContext> _mockFunctionContext;
    private readonly ReprocessDeadLetterQueue _sut;

    public ReprocessDeadLetterQueueTests()
    {
        _mockLogger = new Mock<ILogger<ReprocessDeadLetterQueue>>();
        _mockFactory = new Mock<IServiceBusClientFactory>();
        _mockSender = new Mock<IServiceBusSender>();
        _mockReceiver = new Mock<IServiceBusReceiver>();

        _mockFactory.Setup(f => f.CreateSender(It.IsAny<string>())).Returns(_mockSender.Object);
        _mockFactory.Setup(f => f.CreateReceiver(It.IsAny<string>(), It.IsAny<ServiceBusReceiverOptions>())).Returns(_mockReceiver.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        _mockFunctionContext = new Mock<FunctionContext>();
        _mockFunctionContext.Setup(c => c.InstanceServices).Returns(mockServiceProvider.Object);

        _sut = new ReprocessDeadLetterQueue(_mockLogger.Object, _mockFactory.Object);
    }

    [Fact]
    public async Task RunReprocessDeadLetterQueue_MissingQueueName_ReturnsBadRequest()
    {
        // Arrange
        var request = new FakeHttpRequestData(
            _mockFunctionContext.Object,
            new Uri("http://localhost/api/v1/ReprocessDeadLetterQueue"));

        // Act
        var response = await _sut.RunReprocessDeadLetterQueue(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("queueName", body);
    }

    [Fact]
    public async Task RunReprocessDeadLetterQueue_SuccessfulReplay_ReturnsReplayedCount()
    {
        // Arrange
        var messages = CreateFakeReceivedMessages(3);

        _mockReceiver
            .Setup(r => r.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int maxMessages, TimeSpan? _, CancellationToken _) =>
            {
                // Return messages on first call, empty on second
                var result = messages;
                messages = Array.Empty<ServiceBusReceivedMessage>();
                return result;
            });

        var request = new FakeHttpRequestData(
            _mockFunctionContext.Object,
            new Uri("http://localhost/api/v1/ReprocessDeadLetterQueue?queueName=test-queue"));

        // Act
        var response = await _sut.RunReprocessDeadLetterQueue(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        var json = JsonDocument.Parse(body);
        Assert.Equal("test-queue", json.RootElement.GetProperty("queueName").GetString());
        Assert.Equal(3, json.RootElement.GetProperty("replayed").GetInt32());
        Assert.False(json.RootElement.GetProperty("dryRun").GetBoolean());

        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _mockReceiver.Verify(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RunReprocessDeadLetterQueue_MaxMessagesRespected_StopsAtCap()
    {
        // Arrange - provide 10 messages but cap at 2
        var allMessages = CreateFakeReceivedMessages(10);
        var callCount = 0;

        _mockReceiver
            .Setup(r => r.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int maxMessages, TimeSpan? _, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                    return allMessages.Take(maxMessages).ToList().AsReadOnly();
                return Array.Empty<ServiceBusReceivedMessage>();
            });

        var request = new FakeHttpRequestData(
            _mockFunctionContext.Object,
            new Uri("http://localhost/api/v1/ReprocessDeadLetterQueue?queueName=test-queue&maxMessages=2"));

        // Act
        var response = await _sut.RunReprocessDeadLetterQueue(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        var json = JsonDocument.Parse(body);
        Assert.Equal(2, json.RootElement.GetProperty("replayed").GetInt32());

        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockReceiver.Verify(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RunReprocessDeadLetterQueue_DryRunTrue_UsesPeekAndDoesNotSendOrComplete()
    {
        // Arrange
        var messages = CreateFakeReceivedMessages(3);

        _mockReceiver
            .Setup(r => r.PeekMessagesAsync(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int maxMessages, long? _, CancellationToken _) =>
            {
                var result = messages;
                messages = Array.Empty<ServiceBusReceivedMessage>();
                return result;
            });

        var request = new FakeHttpRequestData(
            _mockFunctionContext.Object,
            new Uri("http://localhost/api/v1/ReprocessDeadLetterQueue?queueName=test-queue&dryRun=true"));

        // Act
        var response = await _sut.RunReprocessDeadLetterQueue(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        var json = JsonDocument.Parse(body);
        Assert.Equal("test-queue", json.RootElement.GetProperty("queueName").GetString());
        Assert.Equal(3, json.RootElement.GetProperty("peeked").GetInt32());
        Assert.True(json.RootElement.GetProperty("dryRun").GetBoolean());

        _mockReceiver.Verify(r => r.PeekMessagesAsync(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockReceiver.Verify(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunReprocessDeadLetterQueue_QueueNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockReceiver
            .Setup(r => r.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ServiceBusException("Entity not found", ServiceBusFailureReason.MessagingEntityNotFound));

        var request = new FakeHttpRequestData(
            _mockFunctionContext.Object,
            new Uri("http://localhost/api/v1/ReprocessDeadLetterQueue?queueName=nonexistent-queue"));

        // Act
        var response = await _sut.RunReprocessDeadLetterQueue(request, _mockFunctionContext.Object);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("nonexistent-queue", body);
        Assert.Contains("not found", body);
    }

    private static ServiceBusReceivedMessage[] CreateFakeReceivedMessages(int count)
    {
        var messages = new ServiceBusReceivedMessage[count];
        for (var i = 0; i < count; i++)
        {
            messages[i] = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData($"message-{i}"),
                messageId: $"msg-{i}");
        }
        return messages;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
