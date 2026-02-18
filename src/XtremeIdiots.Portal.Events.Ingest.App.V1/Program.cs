using System.Reflection;

using Azure.AI.ContentSafety;
using Azure.Identity;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

using XtremeIdiots.Portal.Events.Ingest.App.V1;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Abstractions;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;
using XtremeIdiots.Portal.Events.Ingest.App.V1.Services;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

var host = new HostBuilder()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddLogging();
        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddRepositoryApiClient(options => options
            .WithBaseUrl(configuration["RepositoryApi:BaseUrl"] ?? throw new InvalidOperationException("RepositoryApi:BaseUrl configuration is required"))
            .WithEntraIdAuthentication(configuration["RepositoryApi:ApplicationAudience"] ?? throw new InvalidOperationException("RepositoryApi:ApplicationAudience configuration is required")));

        services.AddMemoryCache();

        services.AddSingleton<IServiceBusClientFactory, ServiceBusClientFactory>();

        // Feature management
        services.AddFeatureManagement();

        // Chat moderation services
        var csEndpoint = configuration["ContentSafety:Endpoint"];
        if (!string.IsNullOrEmpty(csEndpoint))
        {
            services.AddSingleton(_ => new ContentSafetyClient(
                new Uri(csEndpoint),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = configuration["AZURE_CLIENT_ID"]
                })));
        }
        else
        {
            // Register a placeholder so DI doesn't fail when Content Safety is not configured
            services.AddSingleton(_ => new ContentSafetyClient(
                new Uri("https://not-configured.cognitiveservices.azure.com/"),
                new DefaultAzureCredential()));
        }

        services.AddSingleton<IChatModerationService, ChatModerationService>();
        services.AddSingleton<ILocalWordListFilter, LocalWordListFilter>();

        services.AddHealthChecks();
    })
    .Build();

await host.RunAsync();
