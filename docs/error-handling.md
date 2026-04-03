# Error Handling

## Overview

The portal-event-ingest function app processes events through a two-stage pipeline: HTTP ingress → Service Bus queue → queue processor. Error handling is designed to separate **permanently invalid messages** (poison pills) from **transient failures** (API timeouts, network issues) and treat each appropriately.

## HTTP Ingress Validation

The HTTP trigger functions (`PlayerEvents.cs`, `ServerEvents.cs`) validate payloads before enqueuing:

- **JSON format** — malformed JSON returns `400 Bad Request`
- **Required fields** — missing fields return `400 Bad Request` with a descriptive error listing which fields are invalid
- **Enum values** — unrecognised `GameType` values return `400 Bad Request`
- **GUID format** — invalid GUIDs return `400 Bad Request`

This is defence-in-depth — it prevents known-bad messages from entering the queue at all.

### Validation rules per endpoint

| Endpoint | Required fields | Additional validation |
|---|---|---|
| `OnPlayerConnected` | `GameType`, `Guid` | GameType must be valid enum |
| `OnChatMessage` | `GameType`, `Guid` | GameType must be valid enum |
| `OnMapVote` | `GameType`, `Guid`, `MapName` | GameType must be valid enum |
| `OnServerConnected` | `Id`, `GameType` | Id must be valid GUID |
| `OnMapChange` | `ServerId`, `GameName`, `MapName` | ServerId must not be empty GUID |

## Queue Processor Error Classification

Each queue processor (`PlayerEventsIngest.cs`, `ServerEventsIngest.cs`) splits processing into two phases:

### 1. Validation Phase (deserialization + field checks)

If validation fails, the message is **permanently invalid** — it can never succeed regardless of retries. The processor:

1. Logs a **warning** including the full message payload for diagnostics
2. **Returns without throwing** — the Functions runtime auto-completes the message, discarding it

This prevents poison pills from reaching the dead letter queue.

### 2. Processing Phase (API calls, business logic)

If processing fails (e.g. Repository API timeout, network error), the exception **propagates** to the Functions runtime, which:

1. Abandons the message (returns it to the queue)
2. Azure Service Bus retries up to `max_delivery_count` times (configured to 5)
3. After exhausting retries, the message is dead-lettered

### Special cases

| Scenario | Behaviour | Rationale |
|---|---|---|
| `CreatePlayer` returns 409 Conflict | Treated as success, falls through to update | Race condition — another event created the player |
| Player not found (`GetPlayerContext` returns null) | Throws for retry | Player may be created by concurrent `ProcessOnPlayerConnected` |
| Moderation pipeline failure | Caught and logged, processing continues | Chat message was already persisted; moderation is best-effort |

## Service Bus Configuration

### Queue settings (Terraform)

| Setting | Value | Rationale |
|---|---|---|
| `max_delivery_count` | 5 | Sufficient retries for transient failures without excessive noise |
| `lock_duration` | PT5M | Processing involves multiple API calls; 30s default risks re-delivery |

### Trigger settings (host.json)

| Setting | Value | Rationale |
|---|---|---|
| `maxConcurrentCalls` | 8 | B2 plan with 5 processors × 8 = 40 max concurrent API calls |
| `maxAutoLockRenewalDuration` | 00:10:00 | Safety net for long-running processing (moderation + retries) |
| `autoCompleteMessages` | true | Messages complete on success, abandon on exception |

## Resilience

The Repository API client (`api-client-abstractions` / `BaseApi`) provides built-in retry with exponential backoff (3 attempts by default). This is internal to the client library and applies to all API calls automatically.

Circuit breaker functionality is not yet available — it requires an uplift to `api-client-abstractions` (migration from RestSharp to HttpClient).

## Dead Letter Queue Lifecycle

Messages only reach the DLQ when:

1. A **transient failure** (API unavailable, auth failure, network issue) persists across all 5 retry attempts
2. A **player-not-found** condition persists across all retries (the player was never created)

**Poison pills** (bad JSON, invalid fields, unrecognised enums) are **never dead-lettered** — they are logged and discarded at the processor level.

### Replaying dead-lettered messages

The `ReprocessDeadLetterQueue` HTTP function replays messages from the DLQ back to the active queue:

```
POST /api/v1/ReprocessDeadLetterQueue?queueName=chat_message_queue&maxMessages=50&dryRun=false
```

| Parameter | Required | Default | Description |
|---|---|---|---|
| `queueName` | Yes | — | Which queue's DLQ to process |
| `maxMessages` | No | 50 | Maximum messages to replay per invocation |
| `dryRun` | No | false | If true, peek and log without replaying |

The function:
- Fetches messages in batches of 20 with a 1-second delay between batches
- Logs the dead-letter reason, error description, and truncated body for each message
- Returns a JSON response with the count of replayed/peeked messages

### Idempotency considerations

Some Repository API operations are **not idempotent** — retrying them may create duplicates:

| Operation | Idempotent? | Notes |
|---|---|---|
| `CreatePlayer` | No (409 Conflict) | Handled — processor catches conflict |
| `CreateChatMessage` | No (creates duplicates) | Accepted risk — no unique constraint |
| `CreateGameServerEvent` | No (creates duplicates) | Accepted risk — append-only event log |
| `UpsertMapVote` | Yes | True upsert on composite key |
| `CreateAdminAction` | No (creates duplicates) | Mitigated — isolated in moderation try/catch |
