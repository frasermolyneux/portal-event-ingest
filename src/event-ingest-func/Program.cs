using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using XtremeIdiots.Portal.EventsFunc;

using XtremeIdiots.Portal.RepositoryApiClient;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder
            .AddApplicationInsights()
            .AddApplicationInsightsLogger();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        services.AddRepositoryApiClient(options => new RepositoryApiClientOptions(
            config["apim_base_url"] ?? config["repository_base_url"] ?? throw new ArgumentNullException("apim_base_url"),
            config["portal_repository_apim_subscription_key"] ?? throw new ArgumentNullException("portal_repository_apim_subscription_key"),
            config["repository_api_application_audience"] ?? throw new ArgumentNullException("repository_api_application_audience"),
            config["repository_api_path_prefix"] ?? "repository")
        );

        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        services.AddLogging();
        services.AddMemoryCache();
    })
    .Build();

await host.RunAsync();
