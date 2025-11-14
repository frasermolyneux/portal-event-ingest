using Azure.Messaging.ServiceBus;
using Moq;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Services;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Services;

public class ServiceBusSenderWrapperTests
{
    [Fact]
    public async Task SendMessageAsync_CallsUnderlyingSender()
    {
        // Arrange
        var mockSender = new Mock<ServiceBusSender>();
        var wrapper = new ServiceBusSenderWrapper(mockSender.Object);
        var message = new ServiceBusMessage("test message");

        // Act
        await wrapper.SendMessageAsync(message);

        // Assert
        mockSender.Verify(x => x.SendMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullSender_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ServiceBusSenderWrapper(null!));
    }
}
