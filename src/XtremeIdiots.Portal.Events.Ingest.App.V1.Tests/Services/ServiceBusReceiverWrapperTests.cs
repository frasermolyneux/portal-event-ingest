using Azure.Messaging.ServiceBus;
using Moq;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Services;

public class ServiceBusReceiverWrapperTests
{
    [Fact]
    public async Task ReceiveMessagesAsync_CallsUnderlyingReceiver()
    {
        // Arrange
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var wrapper = new ServiceBusReceiverWrapper(mockReceiver.Object);
        var maxMessages = 10;

        // Act
        await wrapper.ReceiveMessagesAsync(maxMessages);

        // Assert
        mockReceiver.Verify(x => x.ReceiveMessagesAsync(maxMessages, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteMessageAsync_CallsUnderlyingReceiver()
    {
        // Arrange
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var wrapper = new ServiceBusReceiverWrapper(mockReceiver.Object);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage();

        // Act
        await wrapper.CompleteMessageAsync(message);

        // Assert
        mockReceiver.Verify(x => x.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_CallsUnderlyingReceiver()
    {
        // Arrange
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var wrapper = new ServiceBusReceiverWrapper(mockReceiver.Object);

        // Act
        await wrapper.CloseAsync();

        // Assert
        mockReceiver.Verify(x => x.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullReceiver_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServiceBusReceiverWrapper(null!));
    }
}
