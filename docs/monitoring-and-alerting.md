# Monitoring and Alerting

## Dashboard

The portal dashboard (`terraform/dashboards/dashboard.json`) provides visibility into:

- **Frontend metrics** — APIM request counts and response codes
- **Service Bus metrics** — incoming/outgoing messages (1-hour view), dead letter queue depth by queue
- **Custom event counts** — OnChatMessage and OnPlayerConnected volumes (48-hour view)
- **Processor metrics** — per-function execution counts, failure counts, success rate
- **Function app health** — failed requests, response time, exceptions, dependency failures, availability

## Alert Rules

### Dead letter messages

**Trigger:** Dead-lettered messages across any queue exceed 10.

| Setting | Value |
|---|---|
| Metric | `DeadletteredMessages` (Microsoft.ServiceBus/namespaces) |
| Aggregation | Maximum |
| Threshold | > 10 |
| Frequency | Every 5 minutes |
| Window | 15 minutes |
| Severity | 2 (Warning) |
| Action group | High |

**What it means:** Messages are failing processing and exhausting retries. Since poison pills are discarded at the processor level, dead-lettered messages indicate genuine transient failures (API outages, auth issues) that persisted across all 5 retry attempts.

**What to do:**
1. Check Application Insights for exception patterns in the `Process*` functions
2. Check Repository API health — is it responding?
3. Check Service Bus metrics for throttling
4. Use `dryRun` on the replay endpoint to inspect DLQ contents:
   ```
   POST /api/v1/ReprocessDeadLetterQueue?queueName=<name>&dryRun=true
   ```
5. Once the root cause is resolved, replay in controlled batches:
   ```
   POST /api/v1/ReprocessDeadLetterQueue?queueName=<name>&maxMessages=50
   ```

### Queue backlog

**Trigger:** Active messages across any queue exceed 1000.

| Setting | Value |
|---|---|
| Metric | `ActiveMessages` (Microsoft.ServiceBus/namespaces) |
| Aggregation | Maximum |
| Threshold | > 1000 |
| Frequency | Every 5 minutes |
| Window | 15 minutes |
| Severity | 2 (Warning) |
| Action group | High |

**What it means:** Queue processors can't keep up with incoming messages, or processors are failing and messages are being re-queued.

**What to do:**
1. Check function app health — is it running?
2. Check processor exception rates in Application Insights
3. Check Repository API latency — slow responses cause backlog
4. Monitor — the backlog should clear once the bottleneck is resolved

### Processor failure rate

**Trigger:** More than 50 failed processor executions in a 5-minute window (production only).

| Setting | Value |
|---|---|
| Data source | Application Insights (KQL query) |
| Query | `requests \| where success == false \| where name startswith "Process"` |
| Threshold | > 50 failures |
| Frequency | Every 5 minutes |
| Window | 5 minutes |
| Severity | 1 (Error) |
| Action group | High |

**What it means:** An elevated rate of processor failures, likely indicating a downstream dependency issue (Repository API outage, auth failure).

**What to do:**
1. Check Application Insights for the specific exception type and function name
2. Check Repository API health and dependency telemetry
3. This alert fires before messages exhaust retries and dead-letter, giving early warning

### Missing OnChatMessage events

**Trigger:** No OnChatMessage custom events ingested for 6 hours (production only).

| Setting | Value |
|---|---|
| Data source | Application Insights (KQL query) |
| Threshold | < 1 event in 6 hours |
| Severity | 0 (Critical) |
| Action group | Critical |

**What it means:** No chat messages are being processed at all — either no game servers are sending events, the HTTP ingress is down, or the queue processor is completely failing.

## Action Groups

Alert notifications are routed through action groups managed in `platform-monitoring`:

| Group | Used for |
|---|---|
| Critical | Missing events, resource health |
| High | Dead letters, queue backlog, failure rate |
| Moderate | (Available for future use) |
| Low | (Available for future use) |
| Informational | Dev environment resource health |

## ReprocessDeadLetterQueue Endpoint

See [Error Handling — Replaying dead-lettered messages](error-handling.md#replaying-dead-lettered-messages) for usage details.

### Response examples

**Normal replay:**
```json
{"queueName": "chat_message_queue", "replayed": 23, "dryRun": false}
```

**Dry run (inspect without replaying):**
```json
{"queueName": "chat_message_queue", "peeked": 23, "dryRun": true}
```

**No messages found:**
```json
{"queueName": "chat_message_queue", "replayed": 0, "dryRun": false}
```
