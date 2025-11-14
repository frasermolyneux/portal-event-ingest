# Copilot Instructions

## Architecture & Data Flow
- Azure Functions app (`net9.0`, isolated worker) hosts HTTP endpoints in `Functions/V1` that accept JSON payloads, validate required fields, and publish to Service Bus queues (`player_connected_queue`, `chat_message_queue`, etc.).
- Matching Service Bus-triggered functions persist events into the Repository API via `XtremeIdiots.Portal.Repository.Api.Client.V1`; follow the HEAD→create/update flow used in `PlayerEventsIngest` to avoid duplicates.
- Event contracts live in `XtremeIdiots.Portal.Events.Abstractions.V1`; new payload shapes should extend `OnEventBase` so they can be packaged and re-used.
- `TelemetryInitializer` sets the Application Insights role name and each ingest handler emits `EventTelemetry`; preserve these calls when extending logic so dashboards stay accurate.

## Configuration Expectations
- Configuration values come from `local.settings.json`, user secrets (`Program.cs` loads assembly secrets), and app settings deployed via Terraform. Use the `Section:Key` casing (`RepositoryApi:BaseUrl`) that maps to `__` in Terraform.
- Service Bus sender functions rely on `DefaultAzureCredential`; locally you must authenticate (e.g., `az login`) or supply env vars matching the connection info. Service Bus triggers use the connection string from `ServiceBusConnection`.
- When adding queues or settings, update both host config and Terraform (`service_bus.tf`, `function_app.tf`) so deployments stay in sync.

## Implementation Patterns
- All queue publishers serialize with `JsonConvert.SerializeObject`; ingest functions immediately deserialize and fail fast with detailed error logs. Follow this defensive pattern for new events.
- `PlayerEventsIngest.GetPlayerId` caches lookups for 15 minutes to avoid redundant API calls. Reuse the same cache key style (`$"{gameType}-${guid}"`) if you expand player-related features.
- Health probing is handled by `HealthCheck` at `/api/health`; any new health dependencies should plug into `HealthCheckService` registrations in `Program.cs`.
- `ReprocessDeadLetterQueue` is the sanctioned path for replaying DLQs; extend it rather than rolling ad-hoc scripts if additional queues need support.

## Tooling & Workflows
- Use the provided VS Code tasks: `build (functions)` cleans then builds the function app, and `test` runs `dotnet test src --filter FullyQualifiedName!~IntegrationTests`.
- GitHub workflows call custom actions (`frasermolyneux/actions`) for .NET CI, Terraform plan/apply, and Function App deployment; reuse these actions in new pipelines instead of inventing alternatives.
- Versioned OpenAPI definitions live under `openapi/` (`EventIngest.openapi+json.json` mirrors the HTTP triggers). Keep these files updated when routes or payloads change so API Management imports remain correct.

## Terraform & Environments
- Terraform state is split via backend files in `terraform/backends/`; choose the correct backend + `tfvars` (`dev.tfvars`, `prd.tfvars`) when reproducing pipeline steps locally.
- `function_app.tf` wires managed identity auth (system-assigned) and Key Vault secret references for Repository API keys. Any new outbound dependency should follow the same Key Vault-backed pattern.
- Service Bus namespace disables shared keys (`local_auth_enabled = false`); all code assumes AAD-based access. Avoid reintroducing connection-string authentication outside of the trigger binding.
