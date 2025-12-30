# Copilot Instructions

## App Shape & Flow
- .NET 9 isolated Azure Functions host wires logging, Application Insights, Repository API client, memory cache, and Service Bus factory in [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Program.cs#L14-L40](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Program.cs#L14-L40).
- HTTP triggers in [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/PlayerEvents.cs#L25-L95](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/PlayerEvents.cs#L25-L95) and [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/ServerEvents.cs#L25-L71](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/ServerEvents.cs#L25-L71) accept JSON, log raw payloads on failure, and enqueue Service Bus messages (`player_connected_queue`, `chat_message_queue`, `map_vote_queue`, `server_connected_queue`, `map_change_queue`).
- Service Bus triggers in [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/PlayerEventsIngest.cs#L38-L218](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/PlayerEventsIngest.cs#L38-L218) and [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/ServerEventsIngest.cs#L25-L70](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/ServerEventsIngest.cs#L25-L70) deserialize, validate required fields, emit `EventTelemetry`, then call Repository API (players, chat, maps, game-server events). Follow the existing HEAD→create/update flow when avoiding duplicates.
- Game server events are persisted verbatim via `CreateGameServerEventDto` for server lifecycle/map changes; player map votes first resolve map metadata before upsert.
- Health probe is exposed at `/health` returning the aggregated `HealthCheckService` status in [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/HealthCheck.cs#L12-L24](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/HealthCheck.cs#L12-L24).
- DLQ replay uses [src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/ReprocessDeadLetterQueue.cs#L19-L79](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/ReprocessDeadLetterQueue.cs#L19-L79); prefer extending it for any dead-letter reprocessing.

## Contracts & Patterns
- Event contracts live under [src/XtremeIdiots.Portal.Events.Abstractions.V1/Models/V1](src/XtremeIdiots.Portal.Events.Abstractions.V1/Models/V1); extend `OnEventBase` and reuse helpers like `ToGameType`/`ToChatType` when adding shapes.
- Defensive JSON handling: HTTP triggers log the raw body then rethrow, and ingest triggers validate every required field before proceeding (see map vote/game type validations in [PlayerEventsIngest](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/PlayerEventsIngest.cs#L152-L197)).
- Player lookups are cached for 15 minutes using the key `$"{gameType}-${guid}"` before calling the Repository API (see `GetPlayerId` in [PlayerEventsIngest](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Functions/V1/PlayerEventsIngest.cs#L200-L217)).
- Application Insights role name is set globally in [src/XtremeIdiots.Portal.Events.Ingest.App.V1/TelemetryInitializer.cs#L6-L12](src/XtremeIdiots.Portal.Events.Ingest.App.V1/TelemetryInitializer.cs#L6-L12) and each handler tracks an `EventTelemetry` instance; preserve these when extending handlers for dashboards.
- Service Bus access is wrapped behind `IServiceBusClientFactory`/`IServiceBusSender`/`IServiceBusReceiver` ([src/XtremeIdiots.Portal.Events.Ingest.App.V1/Services/ServiceBusClientFactory.cs#L1-L32](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Services/ServiceBusClientFactory.cs#L1-L32)) so functions stay testable; prefer adding capabilities via these abstractions.

## Configuration & Identity
- Required settings: `RepositoryApi:BaseUrl`, `RepositoryApi:ApiKey`, `RepositoryApi:ApplicationAudience` plus Application Insights connection string. Missing values throw during startup ([Program](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Program.cs#L24-L33)).
- Service Bus senders use `DefaultAzureCredential` against `ServiceBusConnection:fullyQualifiedNamespace` ([ServiceBusClientFactory](src/XtremeIdiots.Portal.Events.Ingest.App.V1/Services/ServiceBusClientFactory.cs#L12-L24)); triggers use the `ServiceBusConnection` connection string binding.
- Config is pulled from `local.settings.json`, user secrets (loaded in `Program`), and Terraform app settings. Keep `Section:Key` casing to match the `__` convention in Terraform.

## Tooling, Workflows, Infra
- VS Code tasks: `build (functions)` cleans/builds the function app; `test` runs `dotnet test src --filter FullyQualifiedName!~IntegrationTests`; `func` task runs the built functions host from `bin/Debug/net9.0`.
- OpenAPI specs live in `openapi/` and the root `EventIngest.openapi+json.json`; update these when routes/payloads change so API Management stays in sync.
- Terraform: backends per environment under `terraform/backends/` with matching `tfvars` in `terraform/tfvars/`; Function App uses system-assigned identity and Key Vault-backed secrets (`function_app.tf`), and Service Bus disables shared keys (`service_bus.tf` uses AAD). Keep new settings/queues reflected in Terraform and host config together.
