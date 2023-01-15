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

        services.AddRepositoryApiClient(options =>
        {
            options.BaseUrl = config["apim_base_url"] ?? config["repository_base_url"];
            options.ApiKey = config["portal_repository_apim_subscription_key"];
            options.ApiPathPrefix = config["repository_api_path_prefix"] ?? "repository";
        });

        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        services.AddLogging();
        services.AddMemoryCache();
    })
    .Build();

await host.RunAsync();
