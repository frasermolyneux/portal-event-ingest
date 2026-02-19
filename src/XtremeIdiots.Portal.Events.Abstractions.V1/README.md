# XtremeIdiots.Portal.Events.Abstractions.V1

Event contracts and helpers shared by the XtremeIdiots Portal event ingest functions. Provides strongly-typed models for game server events used in the event-driven architecture.

## Installation

```shell
dotnet add package XtremeIdiots.Portal.Events.Abstractions.V1
```

## Event Models

All events inherit from `OnEventBase` which provides:

```csharp
public class OnEventBase
{
    public DateTime EventGeneratedUtc { get; set; }
    public GameType GameType { get; set; }
    public Guid ServerId { get; set; }
}
```

### Available Events

| Event | Additional Properties |
|-------|----------------------|
| `OnChatMessage` | `Username`, `Guid`, `Message`, `Type` |
| `OnPlayerConnected` | `Username`, `Guid`, `IpAddress` |
| `OnServerConnected` | Server connection metadata |
| `OnMapChange` | Map change metadata |
| `OnMapVote` | Map vote metadata |

## Usage

### Publishing Events

```csharp
var chatEvent = new OnChatMessage
{
    EventGeneratedUtc = DateTime.UtcNow,
    GameType = GameType.CallOfDuty4,
    ServerId = serverId,
    Username = "Player1",
    Guid = playerGuid,
    Message = "Hello world",
    Type = ChatType.All
};

await eventPublisher.PublishAsync(chatEvent);
```

### Consuming Events

```csharp
public class ChatMessageHandler
{
    public async Task HandleAsync(OnChatMessage chatEvent)
    {
        Console.WriteLine($"[{chatEvent.GameType}] {chatEvent.Username}: {chatEvent.Message}");
    }
}
```

## License

This project is licensed under the [GPL-3.0-only](https://spdx.org/licenses/GPL-3.0-only.html) license.
