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
- **deploy-prd**: runs on main push + weekly schedule; builds, applies TF, deploys Dev then Prd, verifies deployed version via `/api/v1/info`, imports OpenAPI specs into APIM from live endpoints
- **deploy-dev**: manual dispatch; same pattern as deploy-prd but dev-only
- **codequality**: weekly + PR/push to main (SonarCloud)
- **release-version-and-tag**: runs on main push (src/** changes); uses Nerdbank.GitVersioning (`nbgv`) to calculate SemVer2, builds with `BUILD_VERSION_OVERRIDE`, creates git tag on public releases
- **release-publish-nuget**: triggered after release-version-and-tag completes; publishes Abstractions NuGet package and creates GitHub release
- Workflows use composites from `frasermolyneux/actions` (dotnet-func-ci, terraform-plan, terraform-plan-and-apply, deploy-function-app)

## Versioning

Nerdbank.GitVersioning (`version.json`) drives all versioning. `Directory.Build.props` at repo root sets C# 13 and includes the NBGV package globally. The CI job outputs `build_version` which flows through deploy workflows for version verification.

## OpenAPI & APIM Integration

The OpenAPI spec is generated at runtime by `OpenApiDocumentGenerator` (using `Microsoft.OpenApi` v2.1.0) which reflects over `[Function]`+`[HttpTrigger]` attributes and Abstractions DTOs to build the spec dynamically. It is served at `/api/openapi/v1.json` by the `OpenApi` function. Deploy workflows import specs into APIM from live endpoints via `az apim api import --specification-url`. APIM API definitions are **not** managed by Terraform — Terraform manages only the version set, product, and product policy. The `ApiInfo` function at `/api/v1/info` returns `{ buildVersion }` for deploy-time version verification.

## Terraform

`terraform/` contains dev/prd tfvars and backend configs. Pulls remote state from platform-workloads, platform-monitoring, portal-environments, and portal-core. Provisions Function App, Service Bus namespace/queues, APIM version set/product/product policy, dashboards, alerts, and role assignments. APIM API definitions are managed by deploy workflows, not Terraform. Concurrency groups serialize Dev/Prd applies.

## Conventions

- Prefer managed identity over connection strings
- Health endpoint uses `HealthCheckService` with warning-level logging
- OpenAPI spec generated at runtime by `OpenApiDocumentGenerator` (reflection-based), served at `/api/openapi/v1.json`
- `ApiInfo` function at `/api/v1/info` returns assembly version for deploy verification
- See `docs/development-workflows.md` for branch strategy and CI/CD flow details
