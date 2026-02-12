# Copilot Instructions

## Architecture

.NET 9 isolated Azure Functions app (v4). Entry point is `src/XtremeIdiots.Portal.Events.Ingest.App.V1/Program.cs` which registers Application Insights, the Portal Repository API client, Service Bus client factory, memory cache, and health checks.

## Key Source Paths

- `src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/` — HTTP triggers and queue processors
- `src/XtremeIdiots.Portal.Events.Abstractions.V1/Models/V1/` — shared DTOs
- `src/XtremeIdiots.Portal.Events.Ingest.App.V1/Services/` — Service Bus wrappers
- `src/XtremeIdiots.Portal.Events.Ingest.App.V1.Tests/` — unit tests (xUnit + Moq)
- `terraform/` — infrastructure (Function App, Service Bus, APIM, dashboards, alerts)

## HTTP Ingress

PlayerEvents.cs and ServerEvents.cs expose anonymous endpoints: `/v1/OnPlayerConnected`, `/v1/OnChatMessage`, `/v1/OnMapVote`, `/v1/OnServerConnected`, `/v1/OnMapChange`. Each deserializes DTOs and enqueues to the corresponding Service Bus queue (`player_connected_queue`, `chat_message_queue`, etc.).

## Queue Processors

PlayerEventsIngest.cs and ServerEventsIngest.cs consume queues, validate payloads, fetch/update players and maps via the Portal Repository API, cache player IDs with `IMemoryCache`, and emit `EventTelemetry`. ReprocessDeadLetterQueue.cs replays DLQ messages back to the active queue.

## Service Bus

Use `IServiceBusClientFactory`/`IServiceBusSender`/`IServiceBusReceiver` wrappers. Always `await using` senders and receivers to dispose connections. Inject `IServiceBusClientFactory` in tests instead of creating `ServiceBusClient` directly. Queue names must stay in sync across producers and consumers.

## Configuration

Required settings: `RepositoryApi:BaseUrl`, `RepositoryApi:ApplicationAudience`, `ServiceBusConnection:fullyQualifiedNamespace`. Optional: `ServiceBusConnection:ManagedIdentityClientId`, `AZURE_CLIENT_ID` for managed identity, Application Insights keys. Local dev defaults to Azurite and Service Bus emulator.

## Telemetry

`TelemetryInitializer` sets cloud role name. `host.json` configures sampling with exceptions excluded. Use `EventTelemetry` for domain actions; leave dependency telemetry to SDK defaults.

## Local Development

```bash
dotnet clean src/XtremeIdiots.Portal.Events.Ingest.App.V1/XtremeIdiots.Portal.Events.Ingest.App.V1.csproj
dotnet build src/XtremeIdiots.Portal.Events.Ingest.App.V1/XtremeIdiots.Portal.Events.Ingest.App.V1.csproj
dotnet test src --filter "FullyQualifiedName!~IntegrationTests"
```

## CI/CD

- **build-and-test**: runs on feature/bugfix/hotfix pushes
- **pr-verify**: runs on PRs; dev TF plan by default (skips dependabot/copilot/* unless labeled `run-dev-plan`; prd plan requires `run-prd-plan`)
- **deploy-prd**: runs on main push + weekly schedule; builds, applies TF, deploys Dev then Prd
- **deploy-dev**: manual dispatch
- **codequality**: weekly + PR/push to main (SonarCloud)
- Workflows use composites from `frasermolyneux/actions` (dotnet-func-ci, terraform-plan, terraform-plan-and-apply, deploy-function-app)

## Terraform

`terraform/` contains dev/prd tfvars and backend configs. Pulls remote state from platform-workloads, platform-monitoring, portal-environments, and portal-core. Provisions Function App, Service Bus namespace/queues, APIM artifacts, dashboards, alerts, and role assignments. Concurrency groups serialize Dev/Prd applies.

## Conventions

- Prefer managed identity over connection strings
- Health endpoint uses `HealthCheckService` with warning-level logging
- OpenAPI spec in `openapi/openapi-v1.json`; source snapshot in `EventIngest.openapi+json.json`
- See `docs/development-workflows.md` for branch strategy and CI/CD flow details
